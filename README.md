# .NET 8 分布式微服务
## 技术栈
Ocelot + Consul + Polly + EF Core

### Consul

主动发送心跳包 

`await _client.Agent.PassTTL(serviceId, "ttl");`