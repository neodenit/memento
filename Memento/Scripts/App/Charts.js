$(function () {
    $('#answersPlaceholder').highcharts({
        chart: {
            type: 'column'
        },
        title: {
            text: 'Answers'
        },
        xAxis: {
            categories: chartData.answers.labels,
            crosshair: true
        },
        yAxis: {
            min: 0,
            title: {
                text: 'Answers'
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
            name: 'Answers',
            data: chartData.answers.values
        }]
    });
});