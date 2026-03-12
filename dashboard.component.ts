// src/app/components/dashboard/dashboard.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { LeaveService } from '../../services/leave.service';
import { NotificationService } from '../../services/notification.service';
import { LeaveBalance, LeaveRequestResponse, Notification } from '../../models/models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="min-h-screen bg-gray-50">
      <!-- Navbar -->
      <nav class="bg-white shadow-sm px-6 py-4 flex justify-between items-center">
        <h1 class="text-xl font-bold text-blue-700">Leave Management System</h1>
        <div class="flex items-center gap-4">
          <span class="text-sm text-gray-600">{{ currentUser?.fullName }} ({{ currentUser?.role }})</span>
          <button (click)="logout()" class="text-sm text-red-500 hover:text-red-700 font-medium">Logout</button>
        </div>
      </nav>

      <div class="max-w-6xl mx-auto px-6 py-8">
        <!-- Welcome -->
        <h2 class="text-2xl font-bold text-gray-800 mb-6">Welcome, {{ currentUser?.fullName }}</h2>

        <!-- Balance Cards -->
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
          <div *ngFor="let balance of balances"
            class="bg-white rounded-xl shadow-sm p-5 border-l-4"
            [ngClass]="balance.remainingDays < 3 ? 'border-red-500' : 'border-blue-500'">
            <p class="text-sm text-gray-500">{{ balance.leaveType }}</p>
            <p class="text-3xl font-bold text-gray-800 mt-1">{{ balance.remainingDays }}</p>
            <p class="text-xs text-gray-400 mt-1">{{ balance.usedDays }} used of {{ balance.totalDays }} days</p>
          </div>
        </div>

        <!-- Quick Actions -->
        <div class="flex gap-4 mb-8">
          <a routerLink="/apply-leave"
            class="bg-blue-600 text-white px-5 py-2.5 rounded-lg font-medium hover:bg-blue-700 transition">
            Apply for Leave
          </a>
          <a routerLink="/my-requests"
            class="bg-white text-blue-600 border border-blue-600 px-5 py-2.5 rounded-lg font-medium hover:bg-blue-50 transition">
            My Requests
          </a>
          <a *ngIf="isManager" routerLink="/pending-reviews"
            class="bg-yellow-500 text-white px-5 py-2.5 rounded-lg font-medium hover:bg-yellow-600 transition">
            Pending Reviews
            <span *ngIf="pendingCount > 0" class="ml-2 bg-white text-yellow-600 text-xs rounded-full px-2 py-0.5">{{ pendingCount }}</span>
          </a>
          <a *ngIf="isAdmin" routerLink="/admin"
            class="bg-purple-600 text-white px-5 py-2.5 rounded-lg font-medium hover:bg-purple-700 transition">
            Admin Panel
          </a>
        </div>

        <!-- Recent Requests -->
        <div class="bg-white rounded-xl shadow-sm p-6">
          <h3 class="text-lg font-semibold text-gray-800 mb-4">Recent Leave Requests</h3>
          <div *ngIf="recentRequests.length === 0" class="text-gray-400 text-sm">No requests found</div>
          <table *ngIf="recentRequests.length > 0" class="w-full text-sm">
            <thead>
              <tr class="text-left text-gray-500 border-b">
                <th class="pb-3">Leave Type</th>
                <th class="pb-3">From</th>
                <th class="pb-3">To</th>
                <th class="pb-3">Days</th>
                <th class="pb-3">Status</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let req of recentRequests" class="border-b last:border-0">
                <td class="py-3">{{ req.leaveType }}</td>
                <td class="py-3">{{ req.startDate | date:'dd MMM yyyy' }}</td>
                <td class="py-3">{{ req.endDate | date:'dd MMM yyyy' }}</td>
                <td class="py-3">{{ req.totalDays }}</td>
                <td class="py-3">
                  <span class="px-2 py-1 rounded-full text-xs font-medium"
                    [ngClass]="{
                      'bg-yellow-100 text-yellow-700': req.status === 'Pending',
                      'bg-green-100 text-green-700':  req.status === 'Approved',
                      'bg-red-100 text-red-700':      req.status === 'Rejected',
                      'bg-gray-100 text-gray-600':    req.status === 'Cancelled'
                    }">{{ req.status }}</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  `
})
export class DashboardComponent implements OnInit {
  balances:       LeaveBalance[]        = [];
  recentRequests: LeaveRequestResponse[] = [];
  notifications:  Notification[]        = [];
  pendingCount    = 0;

  constructor(
    public auth:         AuthService,
    private leaveService: LeaveService,
    private notifService: NotificationService
  ) {}

  get currentUser()  { return this.auth.currentUser; }
  get isManager()    { return this.auth.isManager(); }
  get isAdmin()      { return this.auth.isAdmin(); }

  ngOnInit(): void {
    this.leaveService.getMyBalance().subscribe(b => this.balances = b);
    this.leaveService.getMyRequests().subscribe(r => this.recentRequests = r.slice(0, 5));
    if (this.isManager) {
      this.leaveService.getPendingReviews().subscribe(p => this.pendingCount = p.length);
    }
  }

  logout(): void { this.auth.logout(); }
}
