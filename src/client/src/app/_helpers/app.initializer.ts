import { catchError, of } from "rxjs";
import { AuthService } from "../_services/auth.service";

export function appInitializer(authService: AuthService) {
  return () => authService.refreshToken().pipe(catchError(() => of()));
}
