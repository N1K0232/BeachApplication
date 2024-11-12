function auth(language) {
    Alpine.data("auth", () => ({

        firstName: '',
        lastName: '',
        phoneNumber: '',
        email: '',
        password: '',
        confirmPassword: '',
        isBusy: false,
        isPersistent: false,
        twoFactorEnabled: false,
        qrCodeSrc: '',
        twoFactorCode: '',
        errorMessage: '',
        passwordErrorMessage: '',

        clear: function () {
            this.firstName = '';
            this.lastName = '';
            this.phoneNumber = '';
            this.email = '';
            this.password = '';
            this.confirmPassword = '';
            this.isBusy = false;
            this.isPersistent = false;
            this.twoFactorEnabled = false;
            this.qrCodeSrc = '';
            this.twoFactorCode = '',
            this.errorMessage = '';
            this.passwordErrorMessage = '';
        },

        cancel: function () {
            this.clear();
            document.window.href = '/';
        },

        enableTwoFactor: async function () {
            this.isBusy = true;

            try {
                const response = await enable2FAAsync(language);
                const content = await response.json();

                const errorMessage = GetErrorMessage(response.status, content);
                if (errorMessage == null) {
                    window.location.href = '/Accounts/Login';
                }
                else {
                    this.errorMessage = errorMessage;
                }
            } catch (error) {
                this.errorMessage = error.message;
            }
            finally {
                this.isBusy = false;
            }
        },

        forgotPassword: async function () {
            this.isBusy = true;

            try {
                const response = await forgotPasswordAsync(this.email, language);
                if (!response.ok) {
                    const content = await response.json();
                    this.errorMessage = GetErrorMessage(response.status, content);
                }
            }
            catch (error) {
                this.errorMessage = error.message;
            }
            finally {
                this.isBusy = false;
            }
        },

        getQrCode: async function () {
            this.isBusy = true;

            try {
                const response = await getQrCodeAsync(language);
                if (response.ok) {
                    const blob = await response.blob();
                    this.qrCodeSrc = URL.createObjectURL(blob);
                }
                else {
                    const content = await response.content();
                    this.errorMessage = GetErrorMessage(response.status, content);
                }
            } catch (error) {
                this.errormessage = error.message;
            }
            finally {
                this.isBusy = false;
            }
        },

        loadProfile: async function () {
            this.isBusy = true;

            try {
                const response = await loadProfileAsync(language);
                const content = await response.json();

                const errorMessage = GetErrorMessage(response.status, content);
                if (errorMessage == null) {
                }
                else {
                    alert(errorMessage);
                }

            } catch (error) {
                alert(error);
            }
            finally {
                this.isBusy = false;
            }
        },

        login: async function () {
            this.isBusy = true;

            try {
                const response = await loginAsync(this.email, this.password, this.isPersistent, language);
                const content = await response.json();

                const errorMessage = GetErrorMessage(response.status, content);
                if (errorMessage == null) {
                    if (content.token.startsWith('eyJ')) {
                        window.localStorage.setItem('access_token', content.token);
                        window.location.href = '/Dashboard/Products';   
                    }
                    else {
                        window.localStorage.setItem('2fa_token', content.token);
                        window.location.href = '/';
                    }
                }
                else {
                    alert(errorMessage);
                }
            } catch (error) {
                alert(error);
            }
            finally {
                this.isBusy = false;
            }
        },

        register: async function () {
            this.isBusy = true;

            if (!checkPassword(this.password, this.confirmPassword)) {
                this.passwordErrorMessage = "the passwords aren't matching";
                this.isBusy = false;
                return;
            }

            try {

                const response = await registerAsync(this.firstName, this.lastName, this.phoneNumber, this.email, this.password, this.twoFactorEnabled, language);
                if (response.ok) {
                    window.location.href = '/Accounts/Login';
                }
                else {
                    const content = await response.json();
                    this.errorMessage = GetErrorMessage(response.status, content);
                }
                
            }
            catch (error) {
                this.errorMessage = error.message;
            }
            finally {
                this.isBusy = false;
            }
        },

        resetPassword: async function () {
            this.isBusy = true;
            const parameters = new URLSearchParams(window.location.search);

            try {
                const secret = parameters.get('secret');
                const token = parameters.get('token');

                const response = await resetPasswordAsync(secret, token, this.password, language);
                if (response.ok) {
                    window.location.href = '/Accounts/Login';
                }
                else {
                    const content = await response.json();
                    this.errorMessage = GetErrorMessage(response.status, content);
                }
            }
            catch (error) {
                this.errorMessage = error.message;
            }
            finally {
                this.isBusy = false;
            }
        },

        validateTwoFactor: async function () {
            this.isBusy = true;

            try {
                const response = await validateAsync(this.twoFactorCode, language);
                const content = await response.json();

                const errorMessage = GetErrorMessage(response.status, content);
                if (errorMessage == null) {
                    window.localStorage.setItem('access_token', content.token);
                    window.location.href = '/Dashboard/Products';
                }
            } catch (error) {
                this.errorMessage = error.message;
            }
            finally {
                this.isBusy = false;
            }
        },

        verifyEmail: async function () {
            this.isBusy = true;
            const parameters = new URLSearchParams(window.location.search);

            try {
                const secret = parameters.get('secret');
                const token = parameters.get('token');

                const response = await verifyEmailAsync(secret, token, language);
                if (!response.ok) {
                    const content = await response.json();
                    this.errorMessage = GetErrorMessage(response.status, content);
                }
                else {
                    window.location.href = '/Accounts/Login';
                }
            }
            catch (error) {
                this.errorMessage = error.message;
            }
        }
    }));
}

function checkPassword(password, confirmPassword) {
    return confirmPassword.trim().toUpperCase() === password.trim().toUpperCase();
}

async function enable2FAAsync(language) {
    const accessToken = GetAccessToken();
    const response = await fetch('/api/auth/enable2fa', {
        method: "POST",
        headers: {
            "Accept-Language": language,
            "Authorization": `Bearer ${accessToken}`
        }
    });

    return response;
}

async function forgotPasswordAsync(email, language) {
    const request = { email: email };
    const response = await fetch('/api/auth/forgotpassword', {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": language
        },
        body: JSON.stringify(request)
    });

    return response;
}

async function getQrCodeAsync(language) {
    const token = GetTwoFactorToken();
    const response = await fetch('/api/auth/qrcode?token=' + token, {
        method: "GET",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": language
        }
    });

    return response;
}

async function loadProfileAsync(language) {
    const accessToken = GetAccessToken();
    const response = await fetch('/api/auth/profile', {
        method: "GET",
        headers: {
            "Content-type": "application/json",
            "Accept-Language": language,
            "Authorization": `Bearer ${accessToken}`
        }
    });

    return response;
}

async function loginAsync(email, password, isPersistent, language) {

    const request = { email: email, password: password, isPersistent: isPersistent };
    const response = await fetch('/api/auth/login', {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": language
        },
        body: JSON.stringify(request)
    });

    return response;
}

async function registerAsync(firstName, lastName, phoneNumber, email, password, twoFactorEnabled, language) {

    const request = {
        firstName: firstName,
        lastName: lastName,
        phoneNumber: phoneNumber === '' ? null : phoneNumber,
        email: email,
        password: password,
        twoFactorEnabled: twoFactorEnabled
    };

    const response = await fetch('/api/auth/register', {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": language
        },
        body: JSON.stringify(request)
    });

    return response;
}

async function resetPasswordAsync(secret, token, password, language) {

    const request = { secret: secret, token: token, newPassword: password };
    const response = await fetch('/api/auth/resetpassword', {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": language
        },
        body: JSON.stringify(request)
    });

    return response;
}

async function updatePasswordAsync(email, password, language) {

    const token = window.localStorage.getItem('reset_password_token');
    const request = { email: email, password: password, token: token };

    const response = await fetch('/api/auth/updatepassword', {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": language
        },
        body: JSON.stringify(request)
    });

    return response;
}

async function validateAsync(twoFactorCode, language) {

    const token = GetTwoFactorToken();
    const request = { code: twoFactorCode, token: token };

    const response = await fetch('/api/auth/validate2fa', {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": language
        },
        body: JSON.stringify(request)
    });

    return response;
}

async function verifyEmailAsync(secret, token, language) {

    const request = { secret: secret, token: token };
    const response = await fetch('/api/auth/verifyemail', {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": language
        },
        body: JSON.stringify(request)
    });

    return response;
}