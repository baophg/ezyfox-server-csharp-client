﻿using System;
using System.Collections.Generic;
using com.tvd12.ezyfoxserver.client.constant;
using com.tvd12.ezyfoxserver.client.evt;
using com.tvd12.ezyfoxserver.client.handler;
using com.tvd12.ezyfoxserver.client.socket;

namespace com.tvd12.ezyfoxserver.client.manager
{
	public class EzySimpleHandlerManager : EzyHandlerManager
	{

		private readonly EzyClient client;
		private readonly EzyPingSchedule pingSchedule;
		private readonly EzyEventHandlers eventHandlers;
		private readonly EzyDataHandlers dataHandlers;
		private readonly IDictionary<String, EzyAppDataHandlers> appDataHandlerss;

		public EzySimpleHandlerManager(EzyClient client, EzyPingSchedule pingSchedule)
		{
			this.client = client;
			this.pingSchedule = pingSchedule;
			this.eventHandlers = newEventHandlers();
			this.dataHandlers = newDataHandlers();
			this.appDataHandlerss = new Dictionary<String, EzyAppDataHandlers>();
		}

		private EzyEventHandlers newEventHandlers()
		{
			EzyEventHandlers handlers = new EzyEventHandlers(client, pingSchedule);
			handlers.addHandler(EzyEventType.CONNECTION_SUCCESS, new EzyConnectionSuccessHandler());
			handlers.addHandler(EzyEventType.CONNECTION_FAILURE, new EzyConnectionFailureHandler());
			handlers.addHandler(EzyEventType.DISCONNECTION, new EzyDisconnectionHandler());
			return handlers;
		}

		private EzyDataHandlers newDataHandlers()
		{
			EzyDataHandlers handlers = new EzyDataHandlers(client, pingSchedule);
			handlers.addHandler(EzyCommand.PONG, new EzyPongHandler());
			handlers.addHandler(EzyCommand.APP_REQUEST, new EzyAppResponseHandler());
			return handlers;
		}

		public EzyDataHandler getDataHandler(Object cmd)
		{
			return dataHandlers.getHandler(cmd);
		}

		public EzyEventHandler getEventHandler(Object eventType)
		{
			return eventHandlers.getHandler(eventType);
		}

		public EzyAppDataHandlers getAppDataHandlers(String appName)
		{
			EzyAppDataHandlers answer = null;
			if (appDataHandlerss.ContainsKey(appName))
			{
				answer = appDataHandlerss[appName];
			}
			if (answer == null)
			{
				answer = new EzyAppDataHandlers();
				appDataHandlerss[appName] = answer;
			}
			return answer;
		}

		public void addDataHandler(Object cmd, EzyDataHandler dataHandler)
		{
			dataHandlers.addHandler(cmd, dataHandler);
		}

		public void addEventHandler(Object eventType, EzyEventHandler eventHandler)
		{
			eventHandlers.addHandler(eventType, eventHandler);
		}
	}
}