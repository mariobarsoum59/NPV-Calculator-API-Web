import { getTestBed } from '@angular/core/testing';
import {
    BrowserDynamicTestingModule,
    platformBrowserDynamicTesting
} from '@angular/platform-browser-dynamic/testing';
import 'zone.js';
import 'zone.js/testing';

// First, initialize the Angular testing environment.
getTestBed().initTestEnvironment(
    BrowserDynamicTestingModule,
    platformBrowserDynamicTesting()
);

// Import all spec files directly (add your spec files here)
import './app/components/cash-flow-input/cash-flow-input.component.spec';
import './app/components/npv-calculator/npv-calculator.component.spec';
import './app/components/npv-chart/npv-chart.component.spec';
import './app/components/npv-results-table/npv-results-table.component.spec';

