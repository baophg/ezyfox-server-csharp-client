﻿using System;
using System.Collections.Generic;
using com.tvd12.ezyfoxserver.client.request;
using com.tvd12.ezyfoxserver.client.handler;
using com.tvd12.ezyfoxserver.client.entity;
using com.tvd12.ezyfoxserver.client.binding;

namespace com.tvd12.ezyfoxserver.client.support
{
    public class EzyAppProxy
    {
        protected EzyApp app;
        private readonly String appName;
        private readonly EzyBinding binding;
        private readonly EzySocketProxy socketProxy;
        private readonly IDictionary<String, IDictionary<Object, AppProxyDataHandler>>
            appAccessedHandlersByCommand;

        public EzyAppProxy(EzySocketProxy socketProxy, String appName)
        {
            this.appName = appName;
            this.socketProxy = socketProxy;
            this.binding = socketProxy.getBinding();
            this.appAccessedHandlersByCommand =
                new Dictionary<String, IDictionary<Object, AppProxyDataHandler>>();
        }

        public void send(EzyRequest request)
        {
            app.send(request);
        }

        public void send(String cmd)
        {
            app.send(cmd);
        }

        public void send(String cmd, Object data)
        {
            app.send(cmd, binding.marshall<EzyData>(data));
        }

        public void udpSend(EzyRequest request)
        {
            app.udpSend(request);
        }

        public void udpSend(String cmd)
        {
            app.udpSend(cmd);
        }

        public void udpSend(String cmd, Object data)
        {
            app.udpSend(cmd, binding.marshall<EzyData>(data));
        }

        public Object on<T>(String cmd, EzyAppProxyDataHandler<T> handler)
        {
            AppProxyDataHandler dataHandler = (appProxy, data) =>
            {
                handler.Invoke(appProxy, (T)data);
            };
            IDictionary<Object, AppProxyDataHandler> handlers =
                appAccessedHandlersByCommand.ContainsKey(cmd)
                    ? appAccessedHandlersByCommand[cmd]
                    : null;
            if (handlers == null)
            {
                handlers = new Dictionary<Object, AppProxyDataHandler>();
                appAccessedHandlersByCommand[cmd] = handlers;
                socketProxy.getClient()
                    .setup()
                    .setupApp(appName)
                    .addDataHandler(
                        cmd,
                        new AppDataHandler(
                            this,
                            typeof(T),
                            handlers
                        )
                    );
            }
            handlers[handler] = dataHandler;
            return handler;
        }

        public void unbind(String cmd, Object handler)
        {
            if (appAccessedHandlersByCommand.ContainsKey(cmd))
            {
                appAccessedHandlersByCommand[cmd].Remove(handler);
            }
        }

        public class AppDataHandler : EzyAppDataHandler
        {

            private readonly EzyAppProxy parent;
            private readonly EzyBinding binding;
            private readonly Type dataType;
            private readonly IDictionary<Object, AppProxyDataHandler> handlers;

            public AppDataHandler(
                EzyAppProxy parent,
                Type dataType,
                IDictionary<Object, AppProxyDataHandler> handlers
            )
            {
                this.parent = parent;
                this.binding = parent.binding;
                this.dataType = dataType;
                this.handlers = handlers;

            }

            public void handle(EzyApp app, EzyData responseData)
            {
                Object data = binding.unmarshall(responseData, dataType);
                foreach (AppProxyDataHandler handler in handlers.Values)
                {
                    handler.Invoke(parent, data);
                }
            }
        }
    }

    public class EzySimpleAppProxy : EzyAppProxy
    {
        public EzySimpleAppProxy(
            EzySocketProxy socketProxy,
            String appName
        ) : base(socketProxy, appName)
        {
        }

        public void setApp(EzyApp app)
        {
            this.app = app;
        }
    }
}
