function images(language) {
    Alpine.data("images", () => ({
        file: null,
        isBusy: false,
        errorMessage: '',
        images: {
            items: [],
            pageIndex: 0,
            pageSize: 0,
            totalCount: 0,
            hasNextPage: false
        },

        reset: function () {
            this.file = null;
            this.isBusy = false;
            this.errorMessage = '';
        },

        handleFileChange(event) {
            this.file = event.target.files[0];
        },

        getImages: async function () {
            this.isBusy = true;

            try {
                const response = await getImagesAsync(language);
                const content = await response.json();

                this.errorMessage = GetErrorMessage(response.status, content);
                if (this.errorMessage == null) {
                    this.images = content;
                }

            } catch (error) {
                this.errorMessage = error.message;
            }
            finally {
                this.isBusy = false;
            }
        },

        upload: async function () {

            this.isBusy = true;

            if (!this.file) {
                this.errorMessage = 'No file selected';
                this.isBusy = false;
                return;
            }

            try {
                const response = await uploadImageAsync(this.file, language);
                const content = await response.json();
                this.errorMessage = GetErrorMessage(response.status, content);
            } catch (error) {
                this.errorMessage = error.message;
            }
            finally {
                this.isBusy = false;
            }
        }
    }));
}

async function uploadImageAsync(file, language) {
    const formData = new FormData();
    formData.append('file', file);

    const accessToken = GetAccessToken();
    const response = await fetch('/api/images', {
        method: "POST",
        headers: {
            "Accept-Language": language,
            "Authorization": `Bearer ${accessToken}`
        },
        body: formData
    });

    return response;
}

async function getImagesAsync(language) {
    const accessToken = GetAccessToken();
    const response = await fetch('/api/images', {
        method: "GET",
        headers: {
            "Accept-Language": language,
            "Content-Type": "application/json",
            "Authorization": `Bearer ${accessToken}`
        }
    });

    return response;
}