import { useEffect, useState } from "react"
import { api } from "../utils/api";


export default function Home(){
    const [status, setStatus] = useState("checking...");

    useEffect(() => {
        healthCheck();
    }, []);

    const healthCheck = async () => {
        try {
            var result = await api.get("/test");
            setStatus(result.message);
        } catch (error) {
            setStatus("Something went wrong!");
        }
    }

    return(
        <>
            <h1 className="text-2xl font-bold">BoardingBase</h1>
            <p>{status}</p>
        </>
    )
}