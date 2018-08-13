(async function () {

    const chartColors = {
        red: 'rgb(255, 99, 132)',
        orange: 'rgb(255, 159, 64)',
        yellow: 'rgb(255, 205, 86)',
        green: 'rgb(75, 192, 192)',
        blue: 'rgb(54, 162, 235)',
        purple: 'rgb(153, 102, 255)',
        grey: 'rgb(201, 203, 207)'
    };

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/app")
        .build();

    const app = new Vue({
        el: "#app",
        data: {
            stockIndex: null,
            newStockPrice: null,
            header: "Shopping List App",
            portfolioStocks: [],
            stocks: [],
            pieData : {
                datasets: [{
                    data: [],
                    backgroundColor: [
                        'rgb(114,158,206)',
                        'rgb(255,158,74)',
                        'rgb(103,191,92)',
                        'rgb(237,102,93)',
                        'rgb(173,139,201)',
                        'rgb(168,120,110)',
                        'rgb(237,151,202)',
                        'rgb(162,162,162)',
                        'rgb(205,204,93)',
                        'rgb(109,204,218)']
                }],
                labels: []
            }
            /*
            {
                symbol: "MSFT",
                price: 75.54,
                open: 75.12,
                high: 75.54,
                low: 75.12,
                change: 0.42,
                perc: 0.56

            },
            {
                symbol: "AAPL",
                price: 157.96,
                open: 158.44,
                high: 158.44,
                low: 157.96,
                change: -0.48,
                perc: -0.30

            },
            {
                symbol: "GOOG",
                price: 923.42,
                open: 924.54,
                high: 925.37,
                low: 923.42,
                change: -1.12,
                perc: -0.12
            }
            */
        },
        computed: {
            portfolioTotal() {
                return this.portfolioStocks.reduce( (s, a) => {
                    return s + Number(a.totalValue);
                }, 0);
            }
        },
        methods: {
            async addStockPriceChange() {
                await fetch("/price", {
                    method: "PUT",
                    body: JSON.stringify(
                        {
                            Index: this.stockIndex,
                            Price: this.newStockPrice
                        }),
                    headers: {
                        "content-type": "application/json"
                    }
                })
                this.newRestMessage = null;
            }
        }
    });

    connection.on("Send", stock => {
        app.stocks.push(stock);
    });

    connection.on("Update", stock => {
        priceUpdate(stock);
    })

    var priceUpdate = function (stock) {
        let currentStock = app.stocks[stock.index];
        if (currentStock.price !== stock.price) {
            currentStock.lastChange = currentStock.price - stock.price;
            currentStock.price = stock.price;
            if (currentStock.open == 0) currentStock.open = stock.price;
            if (currentStock.low > stock.price || currentStock.low == 0) currentStock.low = stock.price;
            if (currentStock.high < stock.price) currentStock.high = stock.price;
            currentStock.change = currentStock.price - currentStock.open;
            currentStock.perc = (currentStock.change / currentStock.price).toFixed(2);
        }
    }

    var chart = new Chart(document.getElementById("allocChart"),
                {
                    type: 'pie',
                    data: app.pieData,
                    options: {
                        responsive: true
                    }
                });

    var chartUpdate = function () {
        app.pieData.datasets[0].data.length = 0;
        app.pieData.labels.length = 0;

        app.portfolioStocks.forEach(function(element) {
            app.pieData.datasets[0].data.push(element.totalValue);
            app.pieData.labels.push(element.stockName);
          });
        
        chart.update();
    } 

    await connection.start().then(function () {
        connection.invoke("GetAllStocks").then(function (stocks) {
            for (let i = 0; i < stocks.length; i++) {
                app.stocks.push(stocks[i]);
            }
        });
    });

    try {
        const response = await fetch("/portfolio/1", {
            method: "GET",
            headers: {
                "content-type": "application/json"
            }
        });;
        // Vue specific
        const stockContents = await response.json();
        app.portfolioStocks = stockContents;
        chartUpdate();
    } catch {
        alert("Unable to connect to API");
    }

})();