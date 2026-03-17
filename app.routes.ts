import { Routes } from '@angular/router';
import { authGuard, managerGuard } from './guards';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () =>
      import('./login.component').then(m => m.LoginComponent)
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./dashboard.component').then(m => m.DashboardComponent)
  },
  {
    path: 'apply-leave',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./leave-request.component').then(m => m.LeaveRequestComponent)
  },
  {
    path: 'my-requests',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./leave-list.component').then(m => m.LeaveListComponent)
  },
  {
    path: 'pending-reviews',
    canActivate: [authGuard, managerGuard],
    loadComponent: () =>
      import('./leave-list.component').then(m => m.LeaveListComponent)
  },
  { path: '**', redirectTo: 'dashboard' }
];
