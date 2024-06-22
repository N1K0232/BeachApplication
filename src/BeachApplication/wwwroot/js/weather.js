function weather(language) {
    Alpine.data("weather", () => ({
        searchCity: '',
        cityName: '',
        condition: '',
        conditionUrl: '',
        conditionIconUrl: '',
        temperature: 0.0,
        isBusy: false,

        reset: function () {
            this.searchCity = '';
            this.cityName = '';
            this.condition = '';
            this.conditionUrl = '';
            this.conditionIconUrl = '';
            this.temperature = 0.0;
            this.isBusy = false;
        },

        search: async function () {
            this.isBusy = true;

            try {
                var accessToken = window.localStorage.getItem('access_token');
                var response = await fetch(`api/weatherforecast/${this.searchCity}`, {
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
                    this.cityName = content.cityName;
                    this.condition = content.condition;
                    this.conditionUrl = content.conditionUrl;
                    this.conditionIconUrl = content.conditionIconUrl;
                    this.temperature = content.temperature;
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