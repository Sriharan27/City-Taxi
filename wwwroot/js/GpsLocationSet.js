
    let usertype = Context.Session.GetString("UserType");

    let intervalId;
    /*let userType = '@(User.Identity.IsAuthenticated ? "passenger" : "driver")'; // Set based on user role*/

    function updateLocation() {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(function (position) {
                let latitude = position.coords.latitude;
                let longitude = position.coords.longitude;
                
                // Send the location to the backend every 5 seconds
                $.ajax({
                    url: '/Location/UpdateLocation',
                    type: 'POST',
                    data: {
                        latitude: latitude,
                        longitude: longitude,
                        userType: userType // Identify if it's a passenger or driver
                    }
                });
            });
        }
    }

    $(document).ready(function () {
        intervalId = setInterval(updateLocation, 5000); // Update location every 5 seconds
    });

    // Stop the interval when the page is unloaded
    $(window).on('unload', function () {
        clearInterval(intervalId);
    });

