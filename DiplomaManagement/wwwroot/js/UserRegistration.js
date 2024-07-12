$(function () {

/*    $("#UserRegistrationModal").on('hidden.bs.modal', function (e) {
        $("#UserRegistrationModal input[name='CategoryId']").val('0');
    });

    $('.RegisterLink').click(function () {

        $("#UserRegistrationModal input[name='CategoryId']").val($(this).attr('data-categoryId'));

        $("#UserRegistrationModal").modal("show");

    });*/

    $("#UserRegistrationModal input[name = 'AcceptUserAgreement']").click(onAcceptUserAgreementClick);

    $("#UserRegistrationModal button[name = 'register']").prop("disabled", true);

    function onAcceptUserAgreementClick() {
        if ($(this).is(":checked")) {
            $("#UserRegistrationModal button[name = 'register']").prop("disabled", false);
        }
        else {
            $("#UserRegistrationModal button[name = 'register']").prop("disabled", true);
        }
    }

    $("#UserRegistrationModal input[name = 'Email'], #UserRegistrationModal input[name='UserName']").blur(function () {
        var userName = $("#UserRegistrationModal input[name = 'UserName']").val();
        var email = $("#UserRegistrationModal input[name = 'Email']").val();

        if (userName && email) {
            var url = "UserAuth/UserNameOrEmailExists";

            $.ajax({
                type: "GET",
                url: url,
                data: { userName: userName, email: email },
                success: function (data) {
                    if (data == true) {
                        PresentClosableBootstrapAlert("#alert_placeholder_register", "warning", "Invalid Email", "This email address or username has already been registered");
                    }
                    else {
                        CloseAlert("#alert_placeholder_register");
                    }
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    var errorText = "Status: " + xhr.status + " - " + xhr.statusText;

                    PresentClosableBootstrapAlert("#alert_placeholder_register", "danger", "Error!", errorText);

                    console.error(thrownError + '\r\n' + xhr.statusText + '\r\n' + xhr.responseText);

                }
            });
        }
    });


    var registerUserButton = $("#UserRegistrationModal button[name = 'register']").click(onUserRegisterClick);


    function onUserRegisterClick() {
        var url = "UserAuth/RegisterUser";

        var antiForgeryToken = $("#UserRegistrationModal input[name='__RequestVerificationToken']").val();
        var username = $("#UserRegistrationModal input[name='UserName']").val();
        var password = $("#UserRegistrationModal input[name='Password']").val();
        var confirmPassword = $("#UserRegistrationModal input[name='ConfirmPassword']").val();
        var firstName = $("#UserRegistrationModal input[name='FirstName']").val();
        var lastName = $("#UserRegistrationModal input[name='LastName']").val();
        var email = $("#UserRegistrationModal input[name='Email']").val();
        var instituteId = $("#UserRegistrationModal select[name='InstituteId']").val();

        var user = {
            __RequestVerificationToken: antiForgeryToken,
            Username: username,
            Password: password,
            ConfirmPassword: confirmPassword,
            FirstName: firstName,
            LastName: lastName,
            AcceptUserAgreement: true,
            Email: email,
            InstituteId: instituteId
        };

        $.ajax({
            type: "POST",
            url: url,
            data: user,
            success: function (data) {

                var parsed = $.parseHTML(data);

                var hasErrors = $(parsed).find("input[name='RegistrationInValid']").val() == 'true';

                if (hasErrors) {

                    $("#UserRegistrationModal").html(data);
                    var registerUserButton = $("#UserRegistrationModal button[name = 'register']").click(onUserRegisterClick);
                    $("#UserRegistrationModal input[name = 'AcceptUserAgreement']").click(onAcceptUserAgreementClick);

                    $("#UserRegistrationForm").removeData("validator");
                    $("#UserRegistrationForm").removeData("unobtrusiveValidation");
                    $.validator.unobtrusive.parse("#UserRegistrationForm");
                }
                else {
                    location.href = '/Home/Index';
                }

            },
            error: function (xhr, ajaxOptions, thrownError) {
                var errorText = "Status: " + xhr.status + " - " + xhr.statusText;

                PresentClosableBootstrapAlert("#alert_placeholder_register", "danger", "Error!", errorText);

                console.error(thrownError + '\r\n' + xhr.statusText + '\r\n' + xhr.responseText);
            }

        });

    }

});
