import { Component, OnInit, OnDestroy, AfterViewInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

declare var $: any;

@Component({
  selector: 'app-designer',
  templateUrl: './designer.component.html',
  styleUrls: ['./designer.component.css'],
})
export class DesignerComponent implements OnInit, OnDestroy, AfterViewInit {
  accessToken: string | null = null;
  designer: any;
  reportId: string | null = null;
  constructor(private http: HttpClient, private activeRoute: ActivatedRoute) {}

  async ngOnInit(): Promise<void> {
    try {
      this.reportId = this.activeRoute.snapshot.paramMap.get('id');
      console.log('Report ID:', this.reportId);
      this.accessToken = localStorage.getItem(
        'telerik_report_demo_20250406_token'
      );
      this.overrideFetchWithAuth();
      this.overrideAjaxWithAuth();
      this.overridePreviewPayload();
    } catch (error) {
      console.error('Login failed:', error);
      alert('Failed to login.');
    }
  }

  overrideFetchWithAuth(): void {
    const originalFetch = window.fetch;

    window.fetch = (url: any, args: any = {}) => {
      if (args.headers instanceof Headers) {
        args.headers.append('Authorization', 'Bearer ' + this.accessToken);
      } else {
        args.headers = {
          ...(args.headers ?? {}),
          Authorization: 'Bearer ' + this.accessToken,
        };
      }
      return originalFetch(url, args);
    };
  }

  overrideAjaxWithAuth(): void {
    $.ajaxSetup({
      beforeSend: (xhr: any) => {
        xhr.setRequestHeader('Authorization', 'Bearer ' + this.accessToken);
      },
    });
  }

  overridePreviewPayload(): void {
    const originalFetch = window.fetch;

    window.fetch = async (url: any, options: any = {}) => {
      const isTargetUrl =
        typeof url === 'string' &&
        url.includes('api/reportdesigner/data/model');

      if (isTargetUrl && options.method?.toUpperCase() === 'POST') {
        try {
          const originalBody =
            typeof options.body === 'string'
              ? JSON.parse(options.body)
              : options.body;

          if (
            originalBody?.NetType === 'WebServiceDataSource' &&
            originalBody.DataSource
          ) {
            const dataSource = originalBody.DataSource;
            dataSource.Parameters = dataSource.Parameters || [];

            const tokenValue = `Bearer ${this.accessToken}`;
            const existing = dataSource.Parameters.find(
              (p: any) => p?.Name?.toLowerCase() === 'authorization'
            );

            if (existing) {
              existing.Value = tokenValue;
            } else {
              dataSource.Parameters.push({
                Name: 'authorization',
                Value: tokenValue,
                WebServiceParameterType: 'Header',
                NetType: 'WebServiceParameter',
              });
            }

            dataSource.ParameterValues = JSON.stringify({
              authorization: tokenValue,
            });

            options.body = JSON.stringify(originalBody);
            console.log('Modified Payload:', originalBody);
          }
        } catch (err) {
          console.error('Payload auth inject failed:', err);
        }
      }

      return originalFetch.call(this, url, options);
    };
  }

  ngAfterViewInit(): void {
    this.initDesigner();
  }

  initDesigner(): void {
    console.log('Initializing designer...', this.reportId);
    this.designer = $('#webReportDesigner')
      .telerik_WebReportDesigner({
        toolboxArea: { layout: 'list' },
        serviceUrl: 'http://localhost:51864/api/reportdesigner/',
        report: this.reportId,
        error: this.onError,
        persistSession: false,
      })
      .data('telerik_WebReportDesigner');

    $('#toggleHtmlBoxBtn').on('click', () => {
      $("div[title='HtmlTextBox']").toggle();
    });

    $('#saveReport').on('click', () => {
      const saveBtn = $('#webReportDesigner').find(
        "[data-action='documentSave']"
      );
      saveBtn.click();
    });
  }

  onError(e: any, args: any): void {
    if (args.error) {
      console.log(
        `An error occurred! Message: ${args.message}; Error type: ${args.error.constructor.name}`
      );
    } else {
      console.log(`An error occurred! Message: ${args.message};`);
    }
  }

  ngOnDestroy(): void {
    const cleanupLinks = [
      "link[href*='webReportDesignerTheme']",
      "link[href*='webReportDesigner']",
      "link[href*='font/fonticons']",
    ];

    cleanupLinks.forEach((selector) => {
      const links = $(selector);
      if (links.length > 1) links.last().remove();
    });

    $('link').each((_: any, el: any) => {
      const href = $(el).attr('href');
      if (href && href.includes('api/reportdesigner')) {
        $(el).remove();
      }
    });

    this.clearReportDesignerState();
  }

  clearReportDesignerState(): void {
    const keysToRemove = [
      'ToolboxAreaLayout',
      'RecentReports',
      'PreviouslyOpenedReports',
      'LastOpenedReport',
    ];

    keysToRemove.forEach((key) => {
      localStorage.removeItem(key);
      sessionStorage.removeItem(key);
    });

    console.log('✅ Telerik Report Designer state cleared.');
  }
}
