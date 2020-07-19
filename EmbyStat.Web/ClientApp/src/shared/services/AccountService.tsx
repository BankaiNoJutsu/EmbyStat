import jwt from 'jwt-decode';
import moment from 'moment';
import { BehaviorSubject } from 'rxjs';
import { axiosInstance } from './axiosInstance';

import { LoginView, AuthenticateResponse, User } from '../models/login';

const domain = 'account/';
const accessTokenStr = 'accessToken';
const refreshTokenStr = 'refreshToken';

export const userLoggedIn$ = new BehaviorSubject<boolean>(false);

export const login = (login: LoginView): Promise<boolean> => {
  return axiosInstance
    .post<AuthenticateResponse>(`${domain}login`, login)
    .then((response) => {
      setLocalStorage(response.data);
      userLoggedIn$.next(true);
      return true;
    })
    .catch(() => {
      return false;
    });
};

export const logout = () => {
  return axiosInstance.post<boolean>(`${domain}logout`).finally(() => {
    clearLocalStorage();
    userLoggedIn$.next(false);
  });
};

export const register = (register: LoginView): Promise<boolean> => {
  return axiosInstance
    .post<boolean>(`${domain}register`, register)
    .then((response) => {
      return response.data;
    });
};

export const refreshLogin = (
  accessToken: string,
  refreshToken: string
): Promise<boolean> => {
  const refresh = { accessToken, refreshToken };
  return axiosInstance
    .post<AuthenticateResponse>(`${domain}refreshtoken`, refresh)
    .then((response) => {
      if (response.data == null) {
        return false;
      }
      setLocalStorage(response.data);
      return true;
    })
    .catch(() => {
      return false;
    });
};

export const anyAdmins = (): Promise<boolean> => {
  return axiosInstance
    .get<boolean>(`${domain}any`)
    .then(response => {
      return response.data;
    })
    .catch(() => {
      return true;
    });
}

export const resetPassword = (username: string): Promise<boolean> => {
  return axiosInstance
    .post<boolean>(`${domain}password/reset/${username}`)
    .then(response => {
      return response.data;
    })
    .catch(() => {
      return false;
    })
}

export const isUserLoggedIn = async (): Promise<boolean> => {
  let accessToken = localStorage.getItem(accessTokenStr);
  let refreshToken = localStorage.getItem(refreshTokenStr);

  if (
    ['undefined', undefined, null].includes(accessToken) ||
    ['undefined', undefined, null].includes(refreshToken)
  ) {
    userLoggedIn$.next(false);
    return false;
  }

  let tokenExpiration = jwt(accessToken).exp;
  let tokenExpirationTimeInSeconds = tokenExpiration - moment().unix();

  if (tokenExpirationTimeInSeconds < 250) {
    const result = await refreshLogin(
      accessToken as string,
      refreshToken as string
    );
    if (!result) {
      userLoggedIn$.next(false);
      return false;
    }

    accessToken = localStorage.getItem(accessTokenStr);
    refreshToken = localStorage.getItem(refreshTokenStr);
    tokenExpiration = jwt(accessToken).exp;
    tokenExpirationTimeInSeconds = tokenExpiration - moment().unix();

    const newTokenIsValid = tokenExpirationTimeInSeconds > 120;
    userLoggedIn$.next(newTokenIsValid);
    return tokenExpirationTimeInSeconds > 120;
  }

  userLoggedIn$.next(true);
  return true;
};

export const getUserInfo = (): User | null => {
  if (isUserLoggedIn()) {
    const accessToken = localStorage.getItem(accessTokenStr);
    const tokenInfo = jwt(accessToken);
    return {
      username: tokenInfo.sub,
    };
  }

  return null;
};

export const checkUserRoles = (roles: string[]): boolean => {
  const accessToken = localStorage.getItem(accessTokenStr);
  if (accessToken === ('undefined' || undefined || null)) {
    return false;
  }

  const userRoles = jwt(accessToken).roles as string[];
  const duplicates = userRoles.filter((x) => roles.includes(x));
  return duplicates.length > 0;
};

const setLocalStorage = (tokenInfo: AuthenticateResponse) => {
  localStorage.setItem(accessTokenStr, tokenInfo.accessToken);
  localStorage.setItem(refreshTokenStr, tokenInfo.refreshToken);
};

const clearLocalStorage = () => {
  localStorage.removeItem(accessTokenStr);
  localStorage.removeItem(refreshTokenStr);
};
