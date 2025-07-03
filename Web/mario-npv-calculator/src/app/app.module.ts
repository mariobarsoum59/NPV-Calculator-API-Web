import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { AppComponent } from './app.component';
import { CashFlowInputComponent } from './components/cash-flow-input/cash-flow-input.component';
import { ErrorMessageComponent } from './components/error-message/error-message.component';
import { LoadingSpinnerComponent } from './components/loading-spinner/loading-spinner.component';
import { NPVCalculatorComponent } from './components/npv-calculator/npv-calculator.component';
import { NPVChartComponent } from './components/npv-chart/npv-chart.component';
import { NPVResultsTableComponent } from './components/npv-results-table/npv-results-table.component';

import { NgChartsModule } from 'ng2-charts';

import { Chart, registerables } from 'chart.js';
Chart.register(...registerables);

@NgModule({
  declarations: [
    AppComponent,
    NPVCalculatorComponent,
    CashFlowInputComponent,
    NPVChartComponent,
    NPVResultsTableComponent,
    LoadingSpinnerComponent,
    ErrorMessageComponent

  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    ReactiveFormsModule,
    NgChartsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
