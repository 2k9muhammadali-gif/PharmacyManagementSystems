import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { LayoutComponent } from './layout/layout';
import { LoginComponent } from './pages/login/login';
import { DashboardComponent } from './pages/dashboard/dashboard';
import { ProductsComponent } from './pages/products/products';
import { BranchesComponent } from './pages/branches/branches';
import { ManufacturersComponent } from './pages/manufacturers/manufacturers';
import { CustomersComponent } from './pages/customers/customers';
import { SalesComponent } from './pages/sales/sales';
import { InventoryComponent } from './pages/inventory/inventory';
import { ReportsComponent } from './pages/reports/reports';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'products', component: ProductsComponent },
      { path: 'branches', component: BranchesComponent },
      { path: 'manufacturers', component: ManufacturersComponent },
      { path: 'customers', component: CustomersComponent },
      { path: 'sales', component: SalesComponent },
      { path: 'inventory', component: InventoryComponent },
      { path: 'reports', component: ReportsComponent }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];
