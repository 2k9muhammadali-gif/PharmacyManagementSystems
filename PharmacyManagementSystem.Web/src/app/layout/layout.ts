import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterOutlet],
  template: `
    <nav class="navbar navbar-expand-lg navbar-dark bg-primary">
      <div class="container-fluid">
        <a class="navbar-brand" routerLink="/dashboard">Pharmacy MS</a>
        <div class="navbar-nav me-auto">
          <a class="nav-link" routerLink="/dashboard">Dashboard</a>
          <a class="nav-link" routerLink="/products">Products</a>
          <a class="nav-link" routerLink="/manufacturers">Manufacturers</a>
          <a class="nav-link" routerLink="/branches">Branches</a>
          <a class="nav-link" routerLink="/customers">Customers</a>
          <a class="nav-link" routerLink="/sales">Sales</a>
          <a class="nav-link" routerLink="/inventory">Inventory</a>
          <a class="nav-link" routerLink="/reports">Reports</a>
        </div>
        <div class="navbar-nav">
          <span class="nav-link">{{ auth.user()?.fullName }}</span>
          <a class="nav-link" (click)="auth.logout()" href="javascript:void(0)">Logout</a>
        </div>
      </div>
    </nav>
    <div class="container-fluid py-3">
      <router-outlet></router-outlet>
    </div>
  `
})
export class LayoutComponent {
  constructor(public auth: AuthService) {}
}
