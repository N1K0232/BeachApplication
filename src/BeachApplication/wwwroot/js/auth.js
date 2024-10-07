function auth(language) {
    Alpine.data("auth", () => ({
        id: '',
        firstName: '',
        lastName: '',
        email: '',
        password: '',
        confirmPassword: '',
        isBusy: false,
        isPersistent: false,
        passwordErrorMessage: '',

        clear: function () {
            this.id = '';
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
                var request = {
                    email: this.email,
                    password: this.password,
                    isPersistent: this.isPersistent
                };

                var response = await fetch('/api/auth/login', {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "Accept-Language": language
                    },
                    body: JSON.stringify(request)
                });

                var content = await response.json();
                var errorMessage = GetErrorMessage(response.status, content);

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

        me: async function () {
            this.isBusy = true;

            try {
                var accessToken = GetAccessToken();
                var response = await fetch('api/me', {
                    method: "GET",
                    headers: {
                        "Content-type": "application/json",
                        "Accept-Language": language,
                        "Authorization": `Bearer ${accessToken}`
                    }
                });

                var content = await response.json();
                var errorMessage = GetErrorMessage(response.status, content);

                if (errorMessage == null) {
                    this.fullName = `${content.firstName} ${content.lastName}`;
                    this.email = content.email;
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

            const firstName = this.firstName;
            const lastName = this.lastName;
            const email = this.email;
            const password = this.password;

            this.firstName = '';
            this.lastName = '';
            this.email = '';
            this.password = '';

            try {
                var request = {
                    firstName: firstName,
                    lastName: lastName,
                    email: email,
                    password: password
                };

                var response = await fetch('/api/auth/register', {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "Accept-Language": language
                    },
                    body: JSON.stringify(request)
                });

                var content = await response.json();
                var errorMessage = GetErrorMessage(response.status, content);

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
                var request = { email: this.email };
                var response = await fetch('/api/auth/resetpassword', {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "Accept-Language": language
                    },
                    body: JSON.stringify(request)
                });

                var content = await response.json();
                var errorMessage = GetErrorMessage(response.status, content);

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

            const email = this.email;
            const password = this.password;

            this.email = '';
            this.password = '';
            this.confirmPassword = '';

            try {
                var token = window.localStorage.getItem('reset_password_token');
                var request = {
                    email: email,
                    password: password,
                    token: token
                };

                var response = await fetch('/api/auth/updatepassword', {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "Accept-Language": language
                    },
                    body: JSON.stringify(request)
                });

                var content = await response.json();
                var errorMessage = GetErrorMessage(response.status, content);

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