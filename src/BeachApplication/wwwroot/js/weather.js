function weather(language) {
    Alpine.data("weather", () => ({
        city: '',
        isBusy: false,

        reset: function () {
            this.city = '';
            this.isBusy = false;
        },

        search: async function () {
            this.isBusy = true;

            try {
            } catch (error) {
                alert(error);
            }
            finally {
                this.isBusy = false;
            }
        }
    }));
}