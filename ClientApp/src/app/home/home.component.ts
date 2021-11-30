import {Component, Inject} from '@angular/core';
import {HttpClient, HttpHeaders} from "@angular/common/http";

interface SignInUrl {
  url: string
}

@Component({
  selector: 'app-home',
  styleUrls: ['home.component.css'],
  template: `
 <div class="welcome">
    <h1>Welcome</h1>
    <p>This page is used to authenticate with Google for API token generation</p>
    <p>Once you have authenticated with Google the application will provide you with a token you can use to access API resources</p>
   <button><a href="/oauth/signin">Google Authentication</a></button>
 </div>
  `,
})
export class HomeComponent {
  http: HttpClient;
  baseUrl: string;
  signInUrl: SignInUrl;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.http = http;
    this.baseUrl = baseUrl;
  }
}
