function auth(language) {
    Alpine.data("auth", () => ({
        id: '',
        firstName: '',
        lastName: '',
        fullName: '',
        email: '',
        userName: '',
        password: '',
        confirmPassword: '',
        isBusy: false,
        isPersistent: false,
        passwordErrorMessage: '',

        clear: function () {
            this.id = '';
            this.firstName = '';
            this.lastName = '';
            this.fullName = '';
            this.email = '';
            this.userName = '';
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
                    userName: this.userName,
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
                    window.localStorage.setItem('refresh_token', content.refreshToken);
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
                var accessToken = window.localStorage.getItem('access_token');
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

        refresh: async function () {
            this.isBusy = true;

            var accessToken = window.localStorage.getItem('access_token');
            var refreshToken = window.localStorage.getItem('refresh_token');

            try {
                var request = {
                    accessToken: accessToken,
                    refreshToken: refreshToken
                };

                var response = await fetch('/api/auth/refresh', {
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
                    window.localStorage.clear();
                    window.localStorage.setItem('access_token', content.accessToken);
                    window.localStorage.setItem('refresh_token', content.refreshToken);
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

        register: async function () {
            this.isBusy = true;

            if (!this.checkPassword()) {
                this.passwordErrorMessage = "the passwords aren't matching";
                this.isBusy = false;
                return;
            }

            try {
                var request = {
                    firstName: this.firstName,
                    lastName: this.lastName,
                    email: this.email,
                    password: this.password
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
                    window.localStorage.setItem('verify_email_token', content.token);
                    window.location.href = '/Accounts/VerifyEmail';
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
                var request = {
                    email: this.email
                };

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

            if (!this.checkPassword()) {
                this.passwordErrorMessage = "the passwords aren't matching";
                this.isBusy = false;
                return;
            }

            try {
                var token = window.localStorage.getItem('reset_password_token');
                var request = {
                    email: this.email,
                    password: this.password,
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

        verifyEmail: async function () {
            this.isBusy = true;

            try {
                var token = window.localStorage.getItem('verify_email_token');
                var request = {
                    email: this.email,
                    token: token
                };

                var response = await fetch('/api/auth/verifyemail', {
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
            return this.userName.trim().length === 0 && this.password.trim().length === 0;
        },

        invalidRegisterForm: function () {
            return this.firstName.trim().length === 0 && this.lastName.trim().length === 0
                && this.email.trim().length === 0 && this.password.trim().length === 0
                && this.confirmPassword.trim().length === 0;
        },

        checkPassword: function () {
            return this.confirmPassword.trim().toUpperCase() === this.password.trim().toUpperCase();
        }
    }));
}