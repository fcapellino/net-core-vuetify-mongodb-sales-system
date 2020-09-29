<template>
    <v-container fluid>
        <v-col cols="12" xs="12" style="padding-top:0px;">
            <v-card>
                <div v-if="dataLoaded">
                    <canvas id="myChart" height="100"></canvas>
                </div>
                <div v-else class="pa-2">
                    <span>NO DATA AVAILABLE</span>
                </div>
            </v-card>
        </v-col>
    </v-container>
</template>

<script lang="ts">
    import { Chart } from 'chart.js';
    import { Component, Vue } from 'vue-property-decorator';
    import { Green20 } from 'chartjs-plugin-colorschemes/src/colorschemes/colorschemes.tableau';
    import { Notify } from '../common/notify';
    import { SaleService } from '../services/sale.service';
    import { Utils } from '../common/utils';

    @Component({
        methods: {
        }
    })
    export default class SalesReportComponent extends Vue {
        private dataLoaded: Boolean = false;
        private utils: any = Utils;

        private saleService = new SaleService();
        private statistics: Array<any> = [];

        private async mounted() {
            var self = this;
            await self.loadStatistics();
        }
        private async loadStatistics() {
            var self = this;
            try {
                var response = await self.saleService.getAnnualStatistics();
                var error = response?.data?.error;
                if (error === false) {
                    var resources = response.data.resources;
                    self.statistics = resources.itemsList;
                    self.dataLoaded = true;

                    self.$nextTick(() => {
                        var ctx = document.getElementById('myChart');
                        var myChart = new Chart(ctx, {
                            type: 'bar',
                            data: {
                                labels: self.statistics.map(s => s.label),
                                datasets: [{
                                    data: self.statistics.map(s => s.total),
                                    backgroundColor: Green20,
                                    borderWidth: 1
                                }]
                            },
                            options: {
                                title: {
                                    display: true,
                                    text: 'Sales Report'
                                },
                                legend: {
                                    display: false
                                },
                                scales: {
                                    yAxes: [{
                                        ticks: {
                                            beginAtZero: true,
                                            callback: function (value, index, values) {
                                                return `$ ${value}`;
                                            }
                                        }
                                    }]
                                },
                                tooltips: {
                                    callbacks: {
                                        label: function (tooltipItem, data) {
                                            return `$ ${tooltipItem.yLabel}`;
                                        }
                                    }
                                }
                            }
                        });
                    });
                }
            }
            catch (error) {
                Notify.pushErrorNotification('Error. The operation cannot be completed.');
            }
        }
    }
</script>
