$(function () {
    function drawSingleColumnChart(placeholder, labels, values, title, label) {
        $(placeholder).highcharts({
            chart: {
                type: 'column'
            },
            title: {
                text: title
            },
            xAxis: {
                categories: labels,
                crosshair: true
            },
            yAxis: {
                min: 0,
                title: {
                    text: label
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
                name: title,
                color: 'DeepSkyBlue',
                data: values
            }],
            legend: {
                enabled: false
            },
            credits: {
                enabled: false
            }
        });
    }

    function drawDoubleColumnChart(placeholder, labels, correctValues, incorrectValues, correctTitle, incorrectTitle, label) {
        $(placeholder).highcharts({
            chart: {
                type: 'column'
            },
            title: {
                text: correctTitle
            },
            xAxis: {
                categories: labels,
                crosshair: true
            },
            yAxis: {
                min: 0,
                title: {
                    text: label
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
                name: correctTitle,
                color: 'GreenYellow',
                data: correctValues
            }, {
                name: incorrectTitle,
                color: 'Crimson',
                data: incorrectValues
            }],
            legend: {
                enabled: false
            },
            credits: {
                enabled: false
            }
        });
    };

    function drawPieChart(placeholder, oldQuestionCount, newQuestionCount) {
        $(placeholder).highcharts({
            chart: {
                plotBackgroundColor: null,
                plotBorderWidth: null,
                plotShadow: false,
                type: 'pie'
            },
            title: {
                text: 'Progress'
            },
            tooltip: {
                pointFormat: '<b>{point.y}</b>'
            },
            plotOptions: {
                pie: {
                    allowPointSelect: true,
                    cursor: 'pointer',
                    dataLabels: {
                        enabled: false
                    },
                    showInLegend: true
                }
            },
            series: [{
                name: 'Cards',
                colorByPoint: true,
                data: [{
                    name: 'Active questions',
                    color: 'DeepSkyBlue',
                    y: oldQuestionCount
                }, {
                    name: 'New questions',
                    color: 'Black',
                    y: newQuestionCount
                }]
            }]
        });
    }

    drawSingleColumnChart('#answersPlaceholder', chartData.answers.Labels, chartData.answers.Values, 'Total answers', 'Answers');
    drawDoubleColumnChart('#correctAnswersPlaceholder', chartData.correctAnswers.Labels, chartData.correctAnswers.Correct, chartData.correctAnswers.Incorrect, 'Correct answers', 'Incorrect answers', 'Answers');
    drawPieChart('#cardsNumberPlaceholder', chartData.oldQuestionCount, chartData.newQuestionCount);
});