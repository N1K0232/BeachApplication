function auth(language) {
    Alpine.data("auth", () => ({

        firstName: '',
        lastName: '',
        email: '',
        password: '',
        confirmPassword: '',
        isBusy: false,
        isPersistent: false,
        passwordErrorMessage: '',

        clear: function () {
            this.firstName = '';
            this.lastName = '';
            this.email = '';
            this.password = '';
            this.confirmPassword = '';
            this.isBusy = false;
            this.isPersistent = false;
            this.passwordErrorMessage = '';
        },

        cancel: function () {
            this.clear();
            document.window.href = '/';
        },

        login: async function () {
            this.isBusy = true;

            try {
                const response = await loginAsync(this.email, this.password, this.isPersistent, language);
                const content = await response.json();

                const errorMessage = GetErrorMessage(response.status, content);
                if (errorMessage == null) {
                    window.localStorage.setItem('access_token', content.accessToken);
                    window.location.href = '/';
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

        loadProfile: async function () {
            this.isBusy = true;

            try {
                const response = await loadProfileAsync(GetAccessToken(), language);
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

        register: async function () {
            this.isBusy = true;

            if (!checkPassword(this.password, this.confirmPassword)) {
                this.passwordErrorMessage = "the passwords aren't matching";
                this.isBusy = false;
                return;
            }

            try {

                const response = await registerAsync(this.firstName, this.lastName, this.email, this.password, language);
                const content = await response.json();

                const errorMessage = GetErrorMessage(response.status, content);
                if (errorMessage == null) {
                    window.location.href = '/Accounts/Login';
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

        resetPassword: async function () {
            this.isBusy = true;

            try {
                const response = await resetPasswordAsync(this.email, language);
                const content = await response.json();

                const errorMessage = GetErrorMessage(response.status, content);
                if (errorMessage == null) {
                    window.localStorage.setItem('reset_password_token', content.token);
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

        updatePassword: async function () {
            this.isBusy = true;

            if (!checkPassword(this.password, this.confirmPassword)) {
                this.passwordErrorMessage = "the passwords aren't matching";
                this.isBusy = false;
                return;
            }

            try {
                const token = window.localStorage.getItem('reset_password_token');
                const response = await updatePasswordAsync(this.email, token, language);

                const content = await response.json();
                const errorMessage = GetErrorMessage(response.status, content);

                if (errorMessage == null) {
                    window.location.href = '/';
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

        invalidLoginForm: function () {
            return this.email.trim().length === 0 && this.password.trim().length === 0;
        },

        invalidRegisterForm: function () {
            return this.firstName.trim().length === 0 && this.lastName.trim().length === 0
                && this.email.trim().length === 0 && this.password.trim().length === 0
                && this.confirmPassword.trim().length === 0;
        }
    }));
}

function checkPassword(password, confirmPassword) {
    return confirmPassword.trim().toUpperCase() === password.trim().toUpperCase();
}

async function loadProfileAsync(accessToken, language) {
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

async function registerAsync(firstName, lastName, email, password, language) {

    const request = { firstName: firstName, lastName: lastName, email: email, password: password };
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

async function resetPasswordAsync(email, language) {
    const request = { email: email };
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

async function updatePasswordAsync(email, token, language) {

    const request = {
        email: email,
        password: password,
        token: token
    };

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