function categories(language) {
    Alpine.data("categories", () => ({
        name: '',
        description: '',
        categories: [],
        isBusy: false,

        reset: function () {
            this.name = '';
            this.description = '';
            this.categories = [];
            this.isBusy = false;
        },

        delete: async function () {
            this.isBusy = true;

            try {
                var accessToken = GetAccessToken();
                var response = await fetch(`/api/categories/${id}`, {
                    method: "DELETE",
                    headers: {
                        "Content-Type": "application/json",
                        "Accept-Language": language,
                        "Authorization": `Bearer ${accessToken}`
                    }
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

        get: async function (id) {
            this.isBusy = true;

            try {
                var accessToken = GetAccessToken();
                var response = await fetch(`/api/categories/${id}`, {
                    method: "GET",
                    headers: {
                        "Content-Type": "application/json",
                        "Accept-Language": language,
                        "Authorization": `Bearer ${accessToken}`
                    }
                });

                var content = await response.json();
                var errorMessage = GetErrorMessage(response.status, content);

                if (errorMessage == null) {
                    this.name = content.name;
                    this.description = content.description;
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

        getList: async function () {
            this.isBusy = true;

            try {
                var accessToken = GetAccessToken();
                var response = await fetch(`/api/categories?name=${this.name}&description=${this.description}`, {
                    method: "GET",
                    headers: {
                        "Content-Type": "application/json",
                        "Accept-Language": language,
                        "Authorization": `Bearer ${accessToken}`
                    }
                });

                var content = await response.json();
                var errorMessage = GetErrorMessage(response.status, content);

                if (errorMessage == null) {
                    this.categories = content;
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

        insert: async function () {
            this.isBusy = true;

            try {
                var accessToken = GetAccessToken();
                var request = {
                    name: this.name,
                    description: this.description
                };

                this.name = '';
                this.description = '';

                var response = await fetch('/api/categories', {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "Accept-Language": language,
                        "Authorization": `Bearer ${accessToken}`
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

        update: async function () {
            this.isBusy = true;

            try {
                var accessToken = GetAccessToken();
                var request = {
                    name: this.name,
                    description = this.description
                };

                this.name = '';
                this.description = '';

                var response = await fetch(`/api/categories/${id}`, {
                    method: "PUT",
                    headers: {
                        "Content-Type": "application/json",
                        "Accept-Language": language,
                        "Authorization": `Bearer ${accessToken}`
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
        }
    }));
}