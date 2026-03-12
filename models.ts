// src/app/models/models.ts

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  fullName: string;
  role: string;
  userID: number;
}

export interface CreateLeaveRequest {
  leaveTypeID: number;
  startDate: string;
  endDate: string;
  reason: string;
}

export interface LeaveRequestResponse {
  requestID: number;
  employeeName: string;
  leaveType: string;
  startDate: string;
  endDate: string;
  totalDays: number;
  reason: string;
  status: string;
  reviewNote?: string;
  reviewedBy?: string;
  appliedAt: string;
}

export interface LeaveBalance {
  leaveType: string;
  isPaid: boolean;
  totalDays: number;
  usedDays: number;
  remainingDays: number;
  year: number;
}

export interface ReviewRequest {
  status: string;
  reviewNote?: string;
}

export interface Notification {
  notificationID: number;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
}

export interface LeaveType {
  leaveTypeID: number;
  typeName: string;
  maxDaysPerYear: number;
  isPaid: boolean;
}
