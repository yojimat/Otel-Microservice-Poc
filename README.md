# About the project
Boot up with `docker compose up --build`. The main application will be at `localhost:8080`, Zipkin at `localhost:9411` and Jaeger at `localhost:16686`.  
You can also visit the other 2 microservices at `localhost:8081` and `localhost:8082`.  
The first and second project creates a random number and sends down the chain; 0 to 1 and 1 to 2. After visiting the main application, you can see the trace of the 3 microservices at Zipkin and Jaeger.  
 
