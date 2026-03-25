const API_URL = import.meta.env.VITE_API_URL;

export const api = {
    get: async (url: string) => {
        const res = await fetch(`${API_URL}${url}`);
        if(!res.ok) throw new Error("Request failed!");
        return res.json();
    }
};