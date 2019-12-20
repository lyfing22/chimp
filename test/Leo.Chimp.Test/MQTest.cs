using System;
using System.Collections.Generic;
using System.Text;
using Leo.Chimp.Test.Entities;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using Xunit;

namespace Leo.Chimp.Test
{
    public class MQTest
    {
        string qName = "lhtest1";
        string exchangeName = "fanoutchange1"; //Exchange的名称
        string exchangeType = "fanout";//Exchange的类型:direct，fanout，topic，headers 
        string routingKey = "*";
        [Fact]
        public void Insert()
        {
            try
            {
             
                var factory = new ConnectionFactory
                {
                    UserName = "user_producer",
                    Password = "123456",
                    HostName = "192.168.88.39",
                    Port = AmqpTcpEndpoint.UseDefaultPort,
                    VirtualHost = "/",
                    Protocol = Protocols.DefaultProtocol
                };
                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        //设置交换器的类型
                        channel.ExchangeDeclare(exchangeName, exchangeType);
                        //声明一个队列，设置队列是否持久化，排他性，与自动删除
                        channel.QueueDeclare(qName, true, false, false, null);
                        //绑定消息队列，交换器，routingkey
                        channel.QueueBind(qName, exchangeName, routingKey);
                        var properties = channel.CreateBasicProperties();
                        //队列持久化
                        properties.Persistent = true;
                        var school = new School() { Id = Guid.NewGuid(), Name = "初级中学" };
                        var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(school));
                        //发送信息
                        channel.BasicPublish(exchangeName, routingKey, properties, body);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Fact]
        public void Get()
        {
            try
            {
                using (IConnection conn = new ConnectionFactory()
                {
                    HostName = "192.168.88.39",
                    UserName = "user_consumer",
                    Password = "123456"
                    //Port = AmqpTcpEndpoint.UseDefaultPort,
                    //Protocol = Protocols.DefaultProtocol,
                    //VirtualHost = "/"
                }.CreateConnection())
                {
                    using (var channel = conn.CreateModel())
                    {
                        channel.ExchangeDeclare(exchangeName, exchangeType);
                        channel.QueueDeclare(qName, true, false, false, null);
                        channel.QueueBind(qName, exchangeName, routingKey);
                        //定义这个队列的消费者
                        QueueingBasicConsumer consumer = new QueueingBasicConsumer(channel);
                        //false为手动应答，true为自动应答
                        channel.BasicConsume(qName, false, consumer);
                        while (true)
                        {
                            BasicDeliverEventArgs ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                            byte[] bytes = ea.Body;
                            var messageStr = Encoding.UTF8.GetString(bytes);
                            School school=JsonConvert.DeserializeObject<School>(messageStr);
                            //如果是自动应答，下下面这句代码不用写啦。                            
                            if (school != null)
                            {
                                channel.BasicAck(ea.DeliveryTag, false);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }
}
