import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ReportListsComponent } from './report-lists.component';

describe('ReportListsComponent', () => {
  let component: ReportListsComponent;
  let fixture: ComponentFixture<ReportListsComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ReportListsComponent]
    });
    fixture = TestBed.createComponent(ReportListsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
