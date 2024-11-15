function user(language) {
    Alpine.data("user", () => ({
        firstName: '',
        lastName: '',
        email: '',
        errorMessage: '',
        isBusy: false,
        showProfile: false,

        loadProfile: async function () {
            this.isBusy = true;

            try {
                const accessToken = window.localStorage.getItem('access_token');
                if (accessToken == null) {
                    window.location.href = '/Accounts/Login';
                    return;
                }

                const response = await loadProfileAsync(accessToken, language);
                if (response.status === 401 || response.status === 403) {
                    window.localStorage.removeItem('access_token');
                    window.location.href = '/Accounts/Login';
                }
                else {
                    const content = await response.json();
                    this.firstName = content.firstName;
                    this.lastName = content.lastName;
                    this.email = content.email;
                }
            }
            catch (error) {
                alert(error.message);
            }
            finally {
                this.isBusy = false;
            }
        },

        logout: async function () {
            this.isBusy = true;

            try {
                const accessToken = window.localStorage.getItem('access_token');
                const response = await logoutAsync(accessToken, language);

                if (response.ok) {
                    window.localStorage.removeItem('access_token');
                }
            }
            catch (error) {
                alert(error.message);
            }
            finally {
                this.isBusy = false;
            }
        },

        show: function () {
            this.showProfile = true;
        },

        hide: function () {
            this.showProfile = false;
        }
    }));
}

async function loadProfileAsync(accessToken, language) {
    const response = await fetch('/api/me/profile', {
        method: "GET",
        headers: {
            "Authorization": `Bearer ${accessToken}`,
            "Accept-Language": language
        }
    });

    return response;
}

async function logoutAsync(accessToken, language) {
    const response = await fetch('/api/auth/logout', {
        method: "POST",
        headers: {
            "Authorization": `Bearer ${accessToken}`,
            "Accept-Language": language
        }
    });

    return response;
}