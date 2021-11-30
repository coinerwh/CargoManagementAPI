import {Component, Inject, OnInit} from '@angular/core';
import {ActivatedRoute} from "@angular/router";
import {HttpClient} from "@angular/common/http";

@Component({
  selector: 'userinfo-component',
  styleUrls: ['userinfo.component.css'],
  template: `
  <div class="userinfo">
    <h1>User Info</h1>
    <p>This page displays the token provided by Google that authorizes access to protected API resources</p>
    <p>Token:</p>
    <textarea>{{token}}</textarea>
</div>
  `
})
export class UserInfoComponent implements OnInit {
  token: string;
  baseUrl: string;
  http: HttpClient


  constructor(private route: ActivatedRoute, http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.http = http;
    this.baseUrl = baseUrl;
  }

  ngOnInit() {
    this.route.queryParams
      .subscribe(params => {
        this.token = params.token;
      })
  }
}
