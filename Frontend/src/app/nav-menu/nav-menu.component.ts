import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css'],
})
export class NavMenuComponent implements OnInit {
  isExpanded = false;
  isLogin = false;
  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    const token = localStorage.getItem('telerik_report_demo_20250406_token');
    if (token) {
      console.log('Token found in local storage:', token);
      this.isLogin = true;
    } else {
      console.log('No token found in local storage');
      this.isLogin = false;
    }
  }
  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }

  login(): Promise<void> {
    const credentials = {
      username: 'jimkeecn',
      password: 'jimkeecn',
    };

    return this.http
      .post<any>('http://localhost:51864/api/auth/login', credentials)
      .toPromise()
      .then((response) => {
        localStorage.setItem(
          'telerik_report_demo_20250406_token',
          response.token
        );
        console.log('Login successful');
        console.log(localStorage.getItem('telerik_report_demo_20250406_token'));
        this.isLogin = true;
        window.location.reload();
      });
  }

  logout(): void {
    localStorage.removeItem('telerik_report_demo_20250406_token');
    this.isLogin = false;
    window.location.reload();
  }
}
