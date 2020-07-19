import { axiosInstance } from './axiosInstance';
import axios from 'axios';
import {
  MediaServerUdpBroadcast,
  MediaServerLogin,
  MediaServerInfo,
  MediaServerUser,
  Library,
} from '../models/mediaServer';

const domain = 'mediaserver/';

export const searchMediaServers = async (): Promise<
  MediaServerUdpBroadcast[]
> => {
  const embySearch = axiosInstance.get<MediaServerUdpBroadcast>(
    `${domain}server/search?serverType=0`
  );
  const jellyfinSearch = axiosInstance.get<MediaServerUdpBroadcast>(
    `${domain}server/search?serverType=1`
  );

  return axios.all([embySearch, jellyfinSearch]).then(
    axios.spread((...responses) => {
      const servers: MediaServerUdpBroadcast[] = [];
      responses.forEach((response) => {
        if (response.status === 200) {
          servers.push(response.data);
        }
      });

      return servers;
    })
  );
};

export const testApiKey = (login: MediaServerLogin): Promise<boolean | null> => {
  console.log(login);
  return axiosInstance
    .post<boolean>(`${domain}server/test`, login)
    .then((response) => {
      return response.data;
    })
    .catch(() => {
      return null;
    });;
};

export const getServerInfo = (
  forceReSync = false
): Promise<MediaServerInfo | null> => {
  return axiosInstance
    .get<MediaServerInfo>(`${domain}server/info`, {
      params: { forceReSync },
    })
    .then((response) => {
      return response.data;
    })
    .catch(() => {
      console.log('server info error');
      return null;
    });
};

export const getLibraries = (): Promise<Library[] | null> => {
  return axiosInstance.get<Library[]>(`${domain}server/libraries`)
    .then((response) => {
      return response.data;
    })
    .catch(() => {
      return null;
    });
};

export const getAdministrators = (): Promise<MediaServerUser[] | null> => {
  return axiosInstance
    .get<MediaServerUser[]>(`${domain}administrators`)
    .then((response) => {
      return response.data;
    })
    .catch(() => {
      return null;
    });
};
