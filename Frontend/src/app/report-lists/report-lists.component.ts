import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';

interface Report {
  name: string;
  description: string;
}
@Component({
  selector: 'app-report-lists',
  templateUrl: './report-lists.component.html',
  styleUrls: ['./report-lists.component.css'],
})
export class ReportListsComponent implements OnInit {
  newReport: boolean = false;
  form: FormGroup;
  reports: Report[] = [];
  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    public route: Router
  ) {
    this.form = this.fb.group({
      reportName: [''],
      reportWebService: [''],
      reportDescription: [''],
    });
  }

  getReportList() {
    this.http
      .get('http://localhost:51864/api/customreport/get_reports')
      .subscribe((response) => {
        this.reports = response as Report[];
      });
  }

  onSubmit(): void {
    const value = this.form.value;
    console.log('Form submitted:', value);
    this.http
      .post('http://localhost:51864/api/customreport/create', value)
      .subscribe((response) => {
        console.log('Response:', response);
        this.form.reset();
        this.newReport = false;
      });
  }

  onEdit(report: Report): void {
    this.route.navigate(['/designer', report.name]);
  }

  ngOnInit(): void {
    this.getReportList();
  }
}
