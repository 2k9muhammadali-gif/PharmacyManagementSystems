import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="container-fluid py-4">
      <h1 class="mb-4">Dashboard</h1>
      <p class="text-muted">Welcome, {{ auth.user()?.fullName }}</p>
      <div class="row g-3 mb-4">
        <div class="col-md-3">
          <div class="card">
            <div class="card-body">
              <h6 class="text-muted">Branches</h6>
              <h3>{{ branches.length }}</h3>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card">
            <div class="card-body">
              <h6 class="text-muted">Products</h6>
              <h3>{{ productsCount }}</h3>
            </div>
          </div>
        </div>
      </div>
      <div class="card">
        <div class="card-header d-flex justify-content-between align-items-center">
          <span>Quick Links</span>
          <button class="btn btn-sm btn-outline-danger" (click)="logout()">Logout</button>
        </div>
        <div class="card-body">
          <div class="list-group list-group-flush">
            <a routerLink="/branches" class="list-group-item list-group-item-action">Branches</a>
            <a routerLink="/products" class="list-group-item list-group-item-action">Products</a>
            <a routerLink="/manufacturers" class="list-group-item list-group-item-action">Manufacturers</a>
            <a routerLink="/customers" class="list-group-item list-group-item-action">Customers</a>
            <a routerLink="/sales" class="list-group-item list-group-item-action">Sales</a>
            <a routerLink="/inventory" class="list-group-item list-group-item-action">Inventory</a>
          </div>
        </div>
      </div>
    </div>
  `
})
export class DashboardComponent implements OnInit {
  branches: unknown[] = [];
  productsCount = 0;

  constructor(public auth: AuthService, private api: ApiService) {}

  ngOnInit() {
    this.api.get<unknown[]>('/branches').subscribe((b) => (this.branches = b || []));
    this.api.get<unknown[]>('/products').subscribe((p) => (this.productsCount = Array.isArray(p) ? p.length : 0));
  }

  logout() {
    this.auth.logout();
  }
}
