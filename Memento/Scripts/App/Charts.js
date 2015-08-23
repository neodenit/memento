$(function () {
    var drawChart = function (placeholder, labels, values, text) {
        $(placeholder).highcharts({
            chart: {
                type: 'column'
            },
            title: {
                text: text
            },
            xAxis: {
                categories: labels,
                crosshair: true
            },
            yAxis: {
                min: 0,
                title: {
                    text: text
                }
            },
            tooltip: {
                headerFormat: '<span style="font-size:10px">{point.key}</span><table>',
                pointFormat: '<tr><td style="color:{series.color};padding:0">{series.name}: </td>' +
                    '<td style="padding:0"><b>{point.y}</b></td></tr>',
                footerFormat: '</table>',
                shared: true,
                useHTML: true
            },
            plotOptions: {
                column: {
                    pointPadding: 0.2,
                    borderWidth: 0
                }
            },
            series: [{
                name: text,
                data: values
            }],
            legend: {
                enabled: false
            },
            credits: {
                enabled: false
            }
        });
    };

    drawChart('#answersPlaceholder', chartData.answers.labels, chartData.answers.values, 'Answers');
    drawChart('#correctAnswersPlaceholder', chartData.correctAnswers.labels, chartData.correctAnswers.values, 'Correct answers');
    drawChart('#cardsNumberPlaceholder', chartData.cards.labels, chartData.cards.values, 'Active cards');
});