let map;
let marker;
let directionsService;
let directionsRenderer;
let Distance;
let Fare;
let PaymentType;
let PickUpTime;
let DropOffTime;
let checkReservationInterval, dbdriverlocationgpsset;
function initMap() {
    const initialLat = 7.8731;
    const initialLng = 80.7718;

    // Create a map centered at the initial location
    map = new google.maps.Map(document.getElementById('map'), {
        center: { lat: initialLat, lng: initialLng },
        zoom: 15,
    });

    // Create a marker at the initial location
    marker = new google.maps.Marker({
        position: { lat: initialLat, lng: initialLng },
        map: map,
        title: "Your Current Location"
    });

    directionsService = new google.maps.DirectionsService();
    directionsRenderer = new google.maps.DirectionsRenderer();
    directionsRenderer.setMap(map);
}

function updateLocation() {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function (position) {
            const latitude = position.coords.latitude;
            const longitude = position.coords.longitude;

            // Move the marker and update the map's center
            const newPosition = { lat: latitude, lng: longitude };
            marker.setPosition(newPosition);
            map.setCenter(newPosition);

            var driverStatusId = document.getElementById('driver-status-id').value;
            // Send the location to the server
            fetch(`/Driver/UpdateLocation?driverStatusId=${driverStatusId}&latitude=${latitude}&longitude=${longitude}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                }
            })
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Error updating location');
                    }
                    return response.json();
                })
                .then(updatedDriver => {
                    // Log the response to ensure we are getting the updated coordinates
                    console.log('Updated driver location:', updatedDriver);
                })
                .catch(error => {
                    console.error(error);
                });
        });
    } else {
        alert("Geolocation is not supported by this browser.");
    }
}


let currentReservationId = null;

function checkForNewReservation() {
    var driverId = document.getElementById('user-id').value;

    fetch(`/Driver/CheckForNewReservation?driverStatusId=${driverId}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Error checking for new reservations');
            }
            return response.json();
        })
        .then(data => {
            if (data.reservation) {
                console.log('New reservation found:', data);
                currentReservationId = data.reservationId; // Store the reservation ID

                // Show the overlay with reservation details
                document.getElementById('Reservation-ID').textContent = "Reservation ID: " + currentReservationId;
                document.getElementById('Trip-Distance').textContent = "Trip Distance: " + (data.tripDistance || 'N/A') + " Km";
                document.getElementById('Trip-Fare').textContent = "Trip Fare: Rs " + (data.tripFare ? data.tripFare.toFixed(2) : 'N/A');
                document.getElementById('overlay').style.display = 'block';
            }
        })
        .catch(error => {
            console.error(error);
        });
}

document.getElementById('accept-button').onclick = function () {
    if (currentReservationId) {
        updateReservationStatus('Driver Accepted', currentReservationId);
    }
};

document.getElementById('reject-button').onclick = function () {
    if (currentReservationId) {
        updateReservationStatus('Driver Rejected', currentReservationId);
    }
};

function updateReservationStatus(status, reservationId) {
    fetch(`/Driver/UpdateReservationStatus?reservationId=${reservationId}&status=${status}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        }
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Error updating reservation status');
            }
            return response.json();
        })
        .then(data => {

            if (status == "Driver Accepted") {

                console.log("Driver Accepted. Stopping GPS interval.");

                if (!data.pickupLocation) {
                    throw new Error('PickupLocation is undefined');
                }
                // Stop checking for new reservations
                if (checkReservationInterval) {
                    clearInterval(checkReservationInterval);  // Stop checking for new reservations
                }
                if (dbdriverlocationgpsset) {
                    clearInterval(dbdriverlocationgpsset);  // Clear the GPS update interval
                }

                // Update the map with pickup location
                const pickupLocation = data.pickupLocation; // Get pickup location from response
                const [pickupLat, pickupLng] = pickupLocation.split(",").map(Number);
                const pickupPosition = { lat: pickupLat, lng: pickupLng };

                // Update the map with pickup location
                const dropoffLocation = data.dropoffLocation; // Get pickup location from response
                const [dropoffLat, dropoffLng] = dropoffLocation.split(",").map(Number);
                const dropoffPosition = { lat: dropoffLat, lng: dropoffLng };

                // Show directions link for the pickup location
                document.getElementById('directions-link-pickup').setAttribute('href', `https://www.google.com/maps/dir/?api=1&destination=${pickupLat},${pickupLng}`);
                document.getElementById('directions-link-pickup').style.display = 'block';

                // Show directions link for the dropoff location
                document.getElementById('directions-link-dropoff').setAttribute('href', `https://www.google.com/maps/dir/?api=1&destination=${dropoffLat},${dropoffLng}`);
                document.getElementById('directions-link-dropoff').style.display = 'block';

                // Update passenger information
                document.getElementById('passenger-phone').innerHTML = "<strong>Passenger Phone:</strong>  " + data.passengerPhone;
                document.getElementById('trip-distance').innerHTML = "<strong>Trip Distance:</strong> " + data.tripDistance + " Km";
                document.getElementById('trip-fare').innerHTML = "<strong>Trip Fare: Rs</strong> " + data.tripFare.toFixed(2);

                Distance = data.tripDistance;
                Fare = data.tripFare.toFixed(2);
                PaymentType = data.paymentType;

                // Calculate and display the route from pickup to dropoff
                const request = {
                    origin: pickupPosition,
                    destination: dropoffPosition,
                    travelMode: google.maps.TravelMode.DRIVING, 
                };

                directionsService.route(request, (result, status) => {
                    if (status === google.maps.DirectionsStatus.OK) {
                        directionsRenderer.setDirections(result);
                    } else {
                        console.error('Directions request failed due to ' + status);
                    }
                });

                document.getElementById('start-trip').style.display = 'block'; 
                document.getElementById('end-trip').style.display = 'block';
                document.getElementById('passenger-heading').style.display = 'block';

                const driverStatus = "BUSY";

                updateDriverStatus(driverStatus);

            }

            // Hide the overlay after accepting the reservation
            document.getElementById('overlay').style.display = 'none';
        })
        .catch(error => {
            console.error(error);
        });
}

function updateDriverStatus(status)
{
    var driverStatusId = document.getElementById('driver-status-id').value;

    fetch(`/Driver/UpdateDriverStatus?driverStatusId=${driverStatusId}&status=${status}`,{
        method: 'POST',
            headers: {
            'Content-Type': 'application/json',
        }
    })
        .then(response => {
            if (!response.ok) {
                console.error('HTTP Error:', response.status);
                throw new Error('Error updating driver status');
            }
            return response.json();
        })
        .then(data => {
            console.log("Updated status");
        })
        .catch(error => {
            console.error(error);
        });
}
document.getElementById('start-trip').onclick = function ()
{
    tripStatusUpdate("Trip Started");
    document.getElementById('start-trip').style.display = 'none';
    document.getElementById('end-trip').style.display = 'block';
};
document.getElementById('end-trip').onclick = async function () {
    var status = "AVAILABLE";

    updateDriverStatus(status); // No need to await if this doesn't return a promise
    await tripStatusUpdate("Trip Ended"); // Wait for tripStatusUpdate to complete
    showTripSummary(); // This will now execute only after tripStatusUpdate is done
};


async function tripStatusUpdate(status) {
    try {
        const response = await fetch(`/Driver/UpdateReservationStatus?reservationId=${currentReservationId}&status=${status}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            }
        });

        if (!response.ok) {
            throw new Error('Error updating driver status');
        }

        const data = await response.json();
        PickUpTime = formatTimeTo12Hour(data.pickupTime);
        DropOffTime = formatTimeTo12Hour(data.dropoffTime);
        console.log("Updated status");

    } catch (error) {
        console.error('Fetch error: ', error);
    }
}


function showTripSummary() {
    document.getElementById('summary-distance').textContent = Distance;
    document.getElementById('summary-fare').textContent = Fare;
    document.getElementById('summary-payment-type').textContent = PaymentType;
    document.getElementById('pickup-time').textContent = PickUpTime;
    document.getElementById('dropoff-time').textContent = DropOffTime;
    // Show the popup
    document.getElementById('trip-summary-popup').style.display = 'block';
}

// Event listener to close the popup
document.getElementById('close-summary').onclick = function () {
    document.getElementById('trip-summary-popup').style.display = 'none';
    location.reload();
};

function formatTimeTo12Hour(dateTimeString) {
    if (!dateTimeString) {
        return null; // If the input is empty, return null
    }

    // Convert the string to a Date object
    const date = new Date(dateTimeString);

    // Extract hours, minutes, and check for AM/PM
    let hours = date.getHours();
    const minutes = date.getMinutes();
    const ampm = hours >= 12 ? 'PM' : 'AM';

    // Convert hours to 12-hour format
    hours = hours % 12;
    hours = hours ? hours : 12; // If hour is 0, make it 12

    // Pad minutes with leading zero if necessary
    const minutesFormatted = minutes < 10 ? '0' + minutes : minutes;

    // Return formatted time (e.g., "01:37 PM")
    return hours + ':' + minutesFormatted + ' ' + ampm;
}

checkReservationInterval = setInterval(checkForNewReservation, 5000);
window.onload = initMap;
dbdriverlocationgpsset = setInterval(updateLocation, 3000);
