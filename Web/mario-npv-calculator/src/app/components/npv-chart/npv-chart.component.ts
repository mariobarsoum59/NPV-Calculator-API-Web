import { Component, Input, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { ChartConfiguration, ChartType } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { NPVResultItem } from 'src/app/models/npv-result-item.model';


@Component({
  selector: 'app-npv-chart',
  templateUrl: './npv-chart.component.html',
  styleUrls: ['./npv-chart.component.scss']
})
export class NPVChartComponent implements OnChanges {
  @ViewChild(BaseChartDirective) chart?: BaseChartDirective;
  @Input() data: NPVResultItem[] = [];

  public lineChartData: ChartConfiguration['data'] = {
    datasets: [],
    labels: []
  };

  public lineChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: true,
        position: 'top',
      },
      tooltip: {
        callbacks: {
          label: (context) => {
            const value = context.parsed.y;
            const currency = this.data[0]?.currency || 'USD';
            return `NPV: ${currency} ${value.toFixed(2)}`;
          }
        }
      }
    },
    scales: {
      x: {
        title: {
          display: true,
          text: 'Discount Rate (%)'
        }
      },
      y: {
        title: {
          display: true,
          text: 'Net Present Value'
        },
        ticks: {
          callback: function (value) {
            return '$' + value.toLocaleString();
          }
        }
      }
    }
  };

  public lineChartType: ChartType = 'line';

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['data'] && this.data) {
      this.updateChartData();
    }
  }

  private updateChartData(): void {
    const labels = this.data.map(item => item.formattedRate);
    const npvValues = this.data.map(item => item.npv);

    // Find zero crossing for visual indicator
    const zeroLineData = new Array(this.data.length).fill(0);

    this.lineChartData = {
      labels,
      datasets: [
        {
          data: npvValues,
          label: 'NPV',
          borderColor: '#007bff',
          backgroundColor: 'rgba(0, 123, 255, 0.1)',
          borderWidth: 2,
          pointRadius: 3,
          pointHoverRadius: 5,
          tension: 0.2
        },
        {
          data: zeroLineData,
          label: 'Break-even',
          borderColor: '#dc3545',
          borderDash: [5, 5],
          borderWidth: 1,
          pointRadius: 0,
          fill: false
        }
      ]
    };

    this.chart?.update();
  }
}