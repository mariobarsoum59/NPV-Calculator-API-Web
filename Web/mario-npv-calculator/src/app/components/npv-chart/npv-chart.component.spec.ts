import { SimpleChange } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NPVResultItem } from 'src/app/models/npv-result-item.model';
import { NPVChartComponent } from './npv-chart.component';

// Mock ng2-charts module
import { NgChartsModule } from 'ng2-charts';

describe('NPVChartComponent', () => {
  let component: NPVChartComponent;
  let fixture: ComponentFixture<NPVChartComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [NPVChartComponent],
      imports: [NgChartsModule]
    })
      .compileComponents();

    fixture = TestBed.createComponent(NPVChartComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with empty data', () => {
    expect(component.data).toEqual([]);
    expect(component.lineChartData.datasets).toEqual([]);
    expect(component.lineChartData.labels).toEqual([]);
  });

  it('should have line chart type', () => {
    expect(component.lineChartType).toBe('line');
  });

  it('should have chart options configured', () => {
    expect(component.lineChartOptions).toBeDefined();
    expect(component.lineChartOptions?.responsive).toBe(true);
    expect(component.lineChartOptions?.maintainAspectRatio).toBe(false);
  });

  it('should update chart data when input changes', () => {
    spyOn<any>(component, 'updateChartData');
    const testData: NPVResultItem[] = [
      { discountRate: 0.05, npv: 100, formattedRate: '5%', currency: 'USD' }
    ];

    component.ngOnChanges({
      data: new SimpleChange(null, testData, true)
    });

    expect(component['updateChartData']).toHaveBeenCalled();
  });

  it('should not update chart if data does not change', () => {
    spyOn<any>(component, 'updateChartData');

    component.ngOnChanges({
      someOtherProp: new SimpleChange(null, 'value', true)
    });

    expect(component['updateChartData']).not.toHaveBeenCalled();
  });

  it('should update chart data correctly', () => {
    const testData: NPVResultItem[] = [
      { discountRate: 0.05, npv: 100, formattedRate: '5%', currency: 'USD' },
      { discountRate: 0.10, npv: 50, formattedRate: '10%', currency: 'USD' },
      { discountRate: 0.15, npv: -25, formattedRate: '15%', currency: 'USD' }
    ];

    component.data = testData;
    component['updateChartData']();

    expect(component.lineChartData.labels).toEqual(['5%', '10%', '15%']);
    expect(component.lineChartData.datasets.length).toBe(2);
    expect(component.lineChartData.datasets[0].data).toEqual([100, 50, -25]);
    expect(component.lineChartData.datasets[1].data).toEqual([0, 0, 0]);
  });

  it('should set NPV dataset properties', () => {
    component.data = [
      { discountRate: 0.05, npv: 100, formattedRate: '5%', currency: 'USD' }
    ];

    component['updateChartData']();

    const npvDataset = component.lineChartData.datasets[0];
    expect(npvDataset.label).toBe('NPV');
    expect(npvDataset.borderColor).toBe('#007bff');
    expect(npvDataset.borderWidth).toBe(2);
  });

  it('should set break-even line properties', () => {
    component.data = [
      { discountRate: 0.05, npv: 100, formattedRate: '5%', currency: 'USD' }
    ];

    component['updateChartData']();

    const breakEvenDataset = component.lineChartData.datasets[1];
    expect(breakEvenDataset.label).toBe('Break-even');
    expect(breakEvenDataset.borderColor).toBe('#dc3545');
  });

  it('should handle empty data array', () => {
    component.data = [];

    component['updateChartData']();

    expect(component.lineChartData.labels).toEqual([]);
    expect(component.lineChartData.datasets[0].data).toEqual([]);
    expect(component.lineChartData.datasets[1].data).toEqual([]);
  });

  it('should not error if chart is undefined', () => {
    component.chart = undefined;
    component.data = [
      { discountRate: 0.05, npv: 100, formattedRate: '5%', currency: 'USD' }
    ];

    expect(() => component['updateChartData']()).not.toThrow();
  });

  it('should handle single data point', () => {
    component.data = [
      { discountRate: 0.10, npv: 200, formattedRate: '10%', currency: 'EUR' }
    ];

    component['updateChartData']();

    expect(component.lineChartData.labels).toEqual(['10%']);
    expect(component.lineChartData.datasets[0].data).toEqual([200]);
  });
});