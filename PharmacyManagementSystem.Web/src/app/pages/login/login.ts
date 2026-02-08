import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container mt-5">
      <div class="row justify-content-center">
        <div class="col-md-5">
          <div class="card shadow">
            <div class="card-body p-5">
              <h2 class="text-center mb-4">Pharmacy Management System</h2>
              <p class="text-center text-muted mb-4">Sign in to your account</p>
              @if (error) {
                <div class="alert alert-danger">{{ error }}</div>
              }
              @if (loading) {
                <div class="text-center">Loading...</div>
              } @else {
                <form (ngSubmit)="onSubmit()">
                  <div class="mb-3">
                    <label class="form-label">Email</label>
                    <input type="email" class="form-control" [(ngModel)]="email" name="email" required>
                  </div>
                  <div class="mb-3">
                    <label class="form-label">Password</label>
                    <input type="password" class="form-control" [(ngModel)]="password" name="password" required>
                  </div>
                  <button type="submit" class="btn btn-primary w-100">Login</button>
                </form>
              }
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class LoginComponent {
  email = '';
  password = '';
  error = '';
  loading = false;

  constructor(private auth: AuthService, private router: Router) {}

  onSubmit() {
    this.error = '';
    this.loading = true;
    this.auth.login({ email: this.email, password: this.password }).subscribe({
      next: (res) => {
        this.auth.setSession(res);
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.error = err.error?.message || 'Login failed';
        this.loading = false;
      },
      complete: () => (this.loading = false)
    });
  }
}
