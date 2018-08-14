/*jshint esversion: 6 */
(async function () {

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
            pieData: {
                datasets: [{
                    data: [],
                    backgroundColor: [
                        "rgb(114,158,206)",
                        "rgb(255,158,74)",
                        "rgb(103,191,92)",
                        "rgb(237,102,93)",
                        "rgb(173,139,201)",
                        "rgb(168,120,110)",
                        "rgb(237,151,202)",
                        "rgb(162,162,162)",
                        "rgb(205,204,93)",
                        "rgb(109,204,218)"]
                }],
                labels: []
            }
        },
        computed: {
            portfolioTotal() {
                return this.portfolioStocks.reduce((s, a) => {
                    return s + Number(a.totalValue);
                }, 0);
            }
        },
        methods: {
            async updateStockPrice() {
                await fetch("/stock/"+this.stockIndex+"/price", {
                    method: "PUT",
                    body: JSON.stringify(this.newStockPrice),
                    headers: {
                        "content-type": "application/json"
                    }
                });
                this.newRestMessage = null;
            }
        }
    });



    let priceUpdate = function (stock) {
        let currentStock = app.stocks[stock.index];
        if (currentStock.price !== stock.price) {
            currentStock.lastChange = currentStock.price - stock.price;
            currentStock.price = stock.price;
            if (currentStock.open === 0) currentStock.open = stock.price;
            if (currentStock.low > stock.price || currentStock.low === 0) currentStock.low = stock.price;
            if (currentStock.high < stock.price) currentStock.high = stock.price;
            currentStock.change = currentStock.price - currentStock.open;
            currentStock.perc = (currentStock.change / currentStock.price).toFixed(2);
        }
    };

    let chart = new Chart(document.getElementById("allocChart"),
        {
            type: 'pie',
            data: app.pieData,
            options: {
                animation: {
                    animateRotate : false,
                    animateScale : false
                }
            }
        });

    let chartUpdate = async function () {
        app.pieData.datasets[0].data.length = 0;
        app.pieData.labels.length = 0;

        await app.portfolioStocks.forEach(function (element) {
            app.pieData.datasets[0].data.push(element.totalValue);
            app.pieData.labels.push(element.stockName);
        });

        await chart.update();
    };

    let updatePortfolioView = async function () {
        const response = await fetch("/portfolio/1", {
            method: "GET",
            headers: {
                "content-type": "application/json"
            }
        });
        // Vue specific
        app.portfolioStocks = await response.json();
        await chartUpdate();
    };
    
    connection.on("UpdatePortfolio", updatePortfolioView);

    connection.on("Update", stock => {
        priceUpdate(stock);
    });

    await connection.start().then(function () {
        connection.invoke("GetAllStocks").then(function (stocks) {
            for (let i = 0; i < stocks.length; i++) {
                app.stocks.push(stocks[i]);
            }
        });
    });

    try {
        await updatePortfolioView();
    } catch (err) {
        alert("Unable to connect to API");
    }

    console.log(chart);

})();