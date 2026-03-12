// src/app/app.routes.ts
import { Routes } from '@angular/router';
import { authGuard, managerGuard, adminGuard } from './guards/guards';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () =>
      import('./components/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./components/dashboard/dashboard.component').then(m => m.DashboardComponent)
  },
  {
    path: 'apply-leave',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./components/leave-request/leave-request.component').then(m => m.LeaveRequestComponent)
  },
  {
    path: 'my-requests',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./components/leave-list/leave-list.component').then(m => m.LeaveListComponent)
  },
  {
    path: 'pending-reviews',
    canActivate: [authGuard, managerGuard],
    loadComponent: () =>
      import('./components/leave-list/leave-list.component').then(m => m.LeaveListComponent)
  },
  { path: '**', redirectTo: 'dashboard' }
];


// src/app/app.config.ts
import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { jwtInterceptor } from './guards/guards';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([jwtInterceptor]))
  ]
};


// src/app/app.component.ts
import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: `<router-outlet></router-outlet>`
})
export class AppComponent {}
