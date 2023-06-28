import { Role } from './role';

export class Account {
  id?: string;
  firstName?: string;
  lastName?: string;
  username?: string;
  email?: string;
  role?: Role;
  jwt?: string;
}
