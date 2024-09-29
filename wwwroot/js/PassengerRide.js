let map, marker, directionsService, directionsRenderer;
let dropoffAutocomplete, pickupAutocomplete;
let passengerLatitude, passengerLongitude, PricePerKm, TripDistanceInKm, TripFare;
let passengerPickUpLatitude, passengerPickUpLongitude, passengerDropOffLatitude, passengerDropOffLongitude;
let PickUpMarker, dropoffMarker;
let checkReservationInterval;

function initMap() {
    // Initialize the map
    map = new google.maps.Map(document.getElementById('map'), {
        center: { lat: 7.8731, lng: 80.7718 }, // Coordinates for Sri Lanka
        zoom: 8
    });

    directionsService = new google.maps.DirectionsService();
    directionsRenderer = new google.maps.DirectionsRenderer();

    directionsRenderer.setMap(map);

    // Initialize autocomplete
    pickupAutocomplete = new google.maps.places.Autocomplete(document.getElementById('pickup-location'));
    dropoffAutocomplete = new google.maps.places.Autocomplete(document.getElementById('dropoff-location'));
}

function clearDriverSelectionAndFare() {
    // Clear driver selection list
    document.getElementById('nearby-drivers-list').innerHTML = '';
    // Clear fare display
    document.getElementById('fare').innerHTML = '';
}

function SetByGPS(location) {

    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function (position) {
            const latitude = position.coords.latitude;
            const longitude = position.coords.longitude;

            updateLocationFromMap(location, latitude, longitude);
        }, function () {
            alert('Failed to fetch GPS location.');
        });
    } else {
        alert("Geolocation is not supported by this browser.");
    }
}

function SetInMap(location) {
    google.maps.event.clearListeners(map, 'click');
    google.maps.event.addListener(map, 'click', function (event) {
        const latitude = event.latLng.lat();
        const longitude = event.latLng.lng();

        updateLocationFromMap(location, latitude, longitude);
    });
}

function SetByTyping(location) {
    const inputField = location === "Pickup" ? "pickup-location" : "dropoff-location";

    // Enable typing by re-enabling the input field
    document.getElementById(inputField).disabled = false;
    document.getElementById(inputField).focus();

    // Set up Google Places Autocomplete for manual typing
    const autocomplete = new google.maps.places.Autocomplete(document.getElementById(inputField));
    autocomplete.bindTo('bounds', map);

    autocomplete.addListener('place_changed', function () {
        const place = autocomplete.getPlace();
        if (!place.geometry) {
            return; // If no place is selected, exit
        }

        const latitude = place.geometry.location.lat();
        const longitude = place.geometry.location.lng();

        // Reuse the updateLocationFromMap function to handle map updates and markers
        updateLocationFromMap(location, latitude, longitude);
    });
}


function updateLocationFromMap(location, latitude, longitude) {
    const geocoder = new google.maps.Geocoder();
    const latLng = { lat: latitude, lng: longitude };

    if (location == "Pickup") {

        passengerPickUpLatitude = latitude;
        passengerPickUpLongitude = longitude;

        if (PickUpMarker) {
            PickUpMarker.setMap(null); // Remove previous marker from map
        }

        PickUpMarker = new google.maps.Marker({
            position: latLng,
            map: map,
            title: "Pick-up Location",
        });

        map.setCenter(latLng);
        map.setZoom(15);
    }
    else if (location == "Dropoff") {

        passengerDropOffLatitude = latitude;
        passengerDropOffLongitude = longitude;

        if (dropoffMarker) {
            dropoffMarker.setMap(null); // Remove previous marker from map
        }

        dropoffMarker = new google.maps.Marker({
            position: latLng,
            map: map,
            title: "Drop-off Location",
        }); 

        map.setCenter(latLng);
        map.setZoom(15);
    }

    geocoder.geocode({ location: latLng }, function (results, status) {
        if (status === 'OK') {
            if (results[0]) {
                if (location == "Pickup") {
                    document.getElementById('pickup-location').value = results[0].formatted_address;
                    document.getElementById('pickup-location').disabled = true;
                } else if (location == "Dropoff"){
                    document.getElementById('dropoff-location').value = results[0].formatted_address;
                    document.getElementById('dropoff-location').disabled = true;
                }
            }
        } else {
            alert("Geocoding failed: " + status);
        }
    });

    if (passengerPickUpLatitude != null && passengerPickUpLongitude != null && passengerDropOffLatitude != null && passengerDropOffLongitude != null)
    {
        const dropoffLocation = { lat: passengerDropOffLatitude, lng: passengerDropOffLongitude };
        calculateRoute(passengerPickUpLatitude, passengerPickUpLongitude, dropoffLocation);
    }

    google.maps.event.clearListeners(map, 'click');
}
// Fetch nearby drivers by vehicle type
function fetchNearbyDrivers(vehicleType) {
    clearDriverSelectionAndFare();
    const radiusKm = 10; // Set your desired radius in kilometers

    fetch(`/Passenger/GetNearbyDriversByVehicleType?latitude=${passengerPickUpLatitude}&longitude=${passengerPickUpLongitude}&vehicleType=${vehicleType}&radiusKm=${radiusKm}`)
        .then(response => response.json())
        .then(data => {
            updateNearbyDrivers(data.nearbyDrivers);
        })
        .catch(error => {
            console.error('Error fetching nearby drivers:', error);
        });
}
// Update the list of nearby drivers
function updateNearbyDrivers(drivers) {
    const nearbyDriversList = document.getElementById('nearby-drivers-list');
    nearbyDriversList.innerHTML = ''; // Clear the previous list

    drivers.forEach(driver => {
        // Calculate distance between passenger's location and the driver's location
        const driverLocation = new google.maps.LatLng(driver.latitude, driver.longitude);
        PricePerKm = driver.pricePerKM;
        const passengerLocation = new google.maps.LatLng(passengerPickUpLatitude, passengerPickUpLongitude);
        const distanceInMeters = google.maps.geometry.spherical.computeDistanceBetween(passengerLocation, driverLocation);
        const DriverDistanceInKm = (distanceInMeters / 1000).toFixed(2); // Convert to kilometers and round to 2 decimal places

        // Create the list item with driver information and calculated distance
            const li = document.createElement('li');
            li.innerHTML = `<input type="radio" name="driver" value="${driver.driverID}|${driver.vehicleID}|${DriverDistanceInKm}">
                Driver: ${driver.firstName} ${driver.lastName},
                Distance from Pickup: ${DriverDistanceInKm} km`;
            nearbyDriversList.appendChild(li);
    });
}

const nearbyDriversList = document.getElementById('nearby-drivers-list');
nearbyDriversList.addEventListener('change', (event) => {
    if (event.target.name === 'driver') {
        // Calculate fare when a driver is selected
        const dropoffLocation = document.getElementById('dropoff-location').value;
        if (dropoffLocation) {
            const selectedDriver = event.target.value;
            const [driverId, vehicleId, driverDistanceInKm] = selectedDriver.split('|');
            // Call calculateTripFare using the appropriate values
            calculateTripFare(TripDistanceInKm, driverDistanceInKm, PricePerKm);
        }
    }
});
// Calculate and display the fare after selecting a vehicle type and valid route
function calculateTripFare(TripDistanceInKm, driverDistanceInKm, PricePerKm) {
    if (TripDistanceInKm && driverDistanceInKm && PricePerKm) {
        TripFare = (parseFloat(TripDistanceInKm) + parseFloat(driverDistanceInKm)) * parseFloat(PricePerKm);
        document.getElementById('fare').innerText = `Total Fare: LKR ${TripFare.toFixed(2)}`;
    }
}

// Calculate the route from pickup (current location) to drop-off
function calculateRoute(pickupLat, pickupLng, dropoffLocation) {
    const request = {
        origin: new google.maps.LatLng(pickupLat, pickupLng),
        destination: dropoffLocation,
        travelMode: google.maps.TravelMode.DRIVING,
    };

    directionsService.route(request, function (result, status) {
        if (status == google.maps.DirectionsStatus.OK) {
            directionsRenderer.setDirections(result);

            // Calculate the distance and display it
            TripDistanceInKm = result.routes[0].legs[0].distance.text;
            document.getElementById('distance').innerText = "Distance: " + TripDistanceInKm;
            clearDriverSelectionAndFare();
        } else {
            alert("Directions request failed due to " + status);
        }
    });
}

function submitData() {
    const selectedDriver = document.querySelector('input[name="driver"]:checked')?.value;

    if (!selectedDriver) {
        alert("Please select a driver.");
        return;
    }

    const [driverId, vehicleId, driverDistanceInKm] = selectedDriver.split('|');

    if (!passengerPickUpLatitude || !passengerPickUpLongitude) {
        alert("Please provide a valid pickup location.");
        return;
    }

    if (!passengerDropOffLatitude || !passengerDropOffLongitude) {
        alert("Please provide a valid dropoff location.");
        return;
    }

    const cardID = document.getElementById('selected-cardID').value;
    const paymentType = document.getElementById('selected-payment-method').value;

    if (!paymentType || (!cardID && paymentType !== 'Cash')) {
        alert("Please select a valid payment method.");
        return;
    }

    if (!TripDistanceInKm || !TripFare) {
        alert("Trip distance and fare are missing.");
        return;
    }

    const pickupLocation = `${passengerPickUpLatitude},${passengerPickUpLongitude}`;
    const dropoffLocation = `${passengerDropOffLatitude},${passengerDropOffLongitude}`;


    const reservationStatus = null;
    const reservationTime = new Date();
    const pickupTime = null;
    const dropoffTime = null;
    const tripDistance = TripDistanceInKm;
    const tripFare = TripFare;


    const data = {
        PickupLocation: pickupLocation,
        DropoffLocation: dropoffLocation,
        DriverID: parseInt(driverId),  
        VehicleID: parseInt(vehicleId),
        ReservationStatus: reservationStatus,
        ReservationTime: reservationTime.toISOString(),
        PickupTime: pickupTime,
        DropoffTime: dropoffTime,
        CardID: parseInt(cardID),  
        PaymentType: paymentType,
        TripDistance: parseFloat(tripDistance),  
        TripPrice: parseFloat(tripFare)
    };

    
    fetch('/Passenger/SubmitReservation', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(data)
    })
        .then(response => response.json())
        .then(data => {
            if (data && data.reservationID) {
                
                document.getElementById('Reservation-ID').value = data.reservationID;

                alert("Reservation submitted successfully! Your reservation ID is: " + data.reservationID);
                showOverlay();

                var bookRideButton = document.getElementById('bookRideBtn');
                var updateRideButton = document.getElementById('updateRideBtn');

                
                bookRideButton.style.display = 'none'; 
                updateRideButton.style.display = 'inline-block';
                document.getElementById('cancelRideBtn').style.display = 'inline-block';
            } else {
                alert("Reservation submitted, but no reservation ID returned.");
            }

        })
        .catch(error => {
            console.error('Error submitting reservation:', error);
            alert('There was an error submitting your reservation.');
        });
}
//Payment method page extraction
function openPaymentMethod() {
    window.open('Passenger/PaymentMethod', 'PaymentMethodWindow', 'width=600,height=400');
}
function updatePaymentMethod(response) {
    $("#selected-payment-method").val(response.method);
    $("#selected-cardID").val(response.cardID);
    $("#payment-method-btn").text(response.method === "Cash" ? "Cash" : "Card XXXX " + response.lastFourDigits);
}
// Function to show the overlay
function showOverlay() {
    checkReservationStatusInterval = setInterval(checkForReservationStatus, 5000);
    document.getElementById('overlay').classList.remove('hidden'); // Show the overlay
}

// Cancel button functionality
document.getElementById('cancel-button').addEventListener('click', function () {
    var reservationID = document.getElementById('Reservation-ID').value;
    var status = "Passenger Cancelled";
    fetch(`/Passenger/UpdateReservationStatus?reservationId=${reservationID}&status=${status}`, {
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
            console.log("Passenger Cancelled");
        })
        .catch(error => {
            console.error(error);
        });
    hideOverlay(); // Hide the overlay when the cancel button is clicked
});

// Function to hide the overlay
function hideOverlay() {
    clearInterval(checkReservationStatusInterval);
    document.getElementById('overlay').classList.add('hidden'); // Hide the overlay
}

function checkForReservationStatus() {
    var reservationID = document.getElementById('Reservation-ID').value;

    fetch(`/Passenger/checkReservationStatus?reservationId=${reservationID}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Error checking for new reservations');
            }
            return response.json();
        })
        .then(data => {
            if (data.reservationStatus == "Driver Accepted")
            {
                sendTripDetailsSms();
                hideOverlay();
                window.location.href = `/Passenger/RideDetails?reservationId=${reservationID}`;
            }
            else if (data.reservationStatus == "Driver Rejected")
            {
                showDriverRejectedAlert();   
                hideOverlay();
            }
        })
        .catch(error => {
            console.error(error);
        });
}
function showDriverRejectedAlert() {
    var alertDiv = document.getElementById('driver-rejected');
    alertDiv.style.display = 'block'; // Show the alert

    // Set a timeout to fade out the alert after 8 seconds
    setTimeout(function () {
        fadeOut(alertDiv);
    }, 8000); // 8000 milliseconds = 8 seconds
}

function fadeOut(element) {
    var op = 1;  // Initial opacity
    var timer = setInterval(function () {
        if (op <= 0.1) {
            clearInterval(timer);
            element.style.display = 'none';  // Hide the element once faded out
        }
        element.style.opacity = op;  // Set the opacity
        op -= op * 0.1;  // Decrease opacity
    }, 50); // Change opacity every 50 milliseconds
}
function UpdateData() {
    const reservationID = document.getElementById('Reservation-ID').value;
    const selectedDriver = document.querySelector('input[name="driver"]:checked')?.value;

    if (!selectedDriver) {
        alert("Please select a driver.");
        return;
    }

    const [driverId, vehicleId, driverDistanceInKm] = selectedDriver.split('|');

    if (!passengerPickUpLatitude || !passengerPickUpLongitude) {
        alert("Please provide a valid pickup location.");
        return;
    }

    if (!passengerDropOffLatitude || !passengerDropOffLongitude) {
        alert("Please provide a valid dropoff location.");
        return;
    }

    const cardID = document.getElementById('selected-cardID').value;
    const paymentType = document.getElementById('selected-payment-method').value;

    if (!paymentType || (!cardID && paymentType !== 'Cash')) {
        alert("Please select a valid payment method.");
        return;
    }

    if (!TripDistanceInKm || !TripFare) {
        alert("Trip distance and fare are missing.");
        return;
    }

    const pickupLocation = `${passengerPickUpLatitude},${passengerPickUpLongitude}`;
    const dropoffLocation = `${passengerDropOffLatitude},${passengerDropOffLongitude}`;


    const reservationStatus = null;
    const reservationTime = new Date();
    const pickupTime = null;
    const dropoffTime = null;
    const tripDistance = TripDistanceInKm;
    const tripFare = TripFare;


    const data = {
        ReservationID: parseInt(reservationID),
        PickupLocation: pickupLocation,
        DropoffLocation: dropoffLocation,
        DriverID: parseInt(driverId),  // Convert to integer
        VehicleID: parseInt(vehicleId),  // Convert to integer
        ReservationStatus: reservationStatus,
        ReservationTime: reservationTime.toISOString(), // Convert date to ISO string
        PickupTime: pickupTime,
        DropoffTime: dropoffTime,
        CardID: parseInt(cardID),  // Convert to integer if not paying by cash
        PaymentType: paymentType,
        TripDistance: parseFloat(tripDistance),  // Convert to float
        TripPrice: parseFloat(tripFare)  // Convert to float
    };

    // Send data to the server
    fetch('/Passenger/UpdateReservation', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(data)
    })
        .then(response => response.json())
        .then(data => {

                alert("Reservation submitted successfully! Your reservation ID is: " + data.reservationID);
                showOverlay(); // Show overlay after successful reservation

                /*checkReservationStatusInterval = setInterval(checkForReservationStatus, 5000);*/

        })
        .catch(error => {
            console.error('Error submitting reservation:', error);
            alert('There was an error submitting your reservation.');
        });
}
function CancelRide() {
    const reservationID = document.getElementById('Reservation-ID').value;

    fetch(`/Passenger/CancelRide?reservationId=${reservationID}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Error Cancelling reservations');
            }
            return response.json();
        })
        .then(data => {
            alert("Ride Cancelled!");
            window.location.href = `/Passenger`;
        })
        .catch(error => {
            console.error(error);
        });
}
function sendTripDetailsSms() {
    var reservationID = document.getElementById('Reservation-ID').value;

    fetch(`/Passenger/SendTripDetailsSms?reservationId=${reservationID}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        }
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Failed to send SMS');
            }
            console.log('SMS sent successfully!');
        })
        .catch(error => {
            console.error('Error:', error);
        });
}

window.onload = function () {
    initMap();
    SetByGPS("Pickup");
    SetByTyping("Dropoff");
};

