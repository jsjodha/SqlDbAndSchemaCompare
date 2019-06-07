//using Microsoft.Data.Tools.Diagnostics;
//using System;
//using System.Collections;
//using System.Diagnostics;
//using System.Diagnostics.Eventing;
//using System.Globalization;
//using System.Runtime.InteropServices;
//using System.Text;

//namespace SQLDbCompare
//{
//    internal static class Tracer
//    {
//        // Token: 0x17000004 RID: 4
//        // (get) Token: 0x0600006A RID: 106 RVA: 0x00003300 File Offset: 0x00001500
//        internal static TraceSource TraceSource
//        {
//            get
//            {
//                if (Tracer._traceSource != null)
//                {
//                    return Tracer._traceSource;
//                }
//                TraceSource value = new TraceSource("Microsoft.Data.Tools.Diagnostics.Tracer");
//                Interlocked.CompareExchange<TraceSource>(ref Tracer._traceSource, value, null);
//                return Tracer._traceSource;
//            }
//        }

//        // Token: 0x0600006B RID: 107 RVA: 0x00003338 File Offset: 0x00001538
//        private static WindowsEventTracingLevel TraceEventTypeToLevel(TraceEventType eventType)
//        {
//            if (eventType <= TraceEventType.Start)
//            {
//                if (eventType <= TraceEventType.Information)
//                {
//                    switch (eventType)
//                    {
//                        case TraceEventType.Critical:
//                            return WindowsEventTracingLevel.Critical;
//                        case TraceEventType.Error:
//                            return WindowsEventTracingLevel.Error;
//                        case (TraceEventType)3:
//                            return WindowsEventTracingLevel.Always;
//                        case TraceEventType.Warning:
//                            return WindowsEventTracingLevel.Warning;
//                        default:
//                            if (eventType != TraceEventType.Information)
//                            {
//                                return WindowsEventTracingLevel.Always;
//                            }
//                            break;
//                    }
//                }
//                else
//                {
//                    if (eventType == TraceEventType.Verbose)
//                    {
//                        return WindowsEventTracingLevel.Verbose;
//                    }
//                    if (eventType != TraceEventType.Start)
//                    {
//                        return WindowsEventTracingLevel.Always;
//                    }
//                }
//            }
//            else if (eventType <= TraceEventType.Suspend)
//            {
//                if (eventType != TraceEventType.Stop && eventType != TraceEventType.Suspend)
//                {
//                    return WindowsEventTracingLevel.Always;
//                }
//            }
//            else if (eventType != TraceEventType.Resume && eventType != TraceEventType.Transfer)
//            {
//                return WindowsEventTracingLevel.Always;
//            }
//            return WindowsEventTracingLevel.Informational;
//        }

//        // Token: 0x0600006C RID: 108 RVA: 0x000033B8 File Offset: 0x000015B8
//        private static bool WriteEtwEvent(TraceEventType eventType, TraceId traceId, string message)
//        {
//            bool result;
//            switch (Tracer.TraceEventTypeToLevel(eventType))
//            {
//                case WindowsEventTracingLevel.Error:
//                    result = EtwProvider.EventWriteLogError((uint)traceId, message);
//                    break;
//                case WindowsEventTracingLevel.Warning:
//                    result = EtwProvider.EventWriteLogWarning((uint)traceId, message);
//                    break;
//                case WindowsEventTracingLevel.Informational:
//                    result = EtwProvider.EventWriteLogInformational((uint)traceId, message);
//                    break;
//                case WindowsEventTracingLevel.Verbose:
//                    result = EtwProvider.EventWriteLogVerbose((uint)traceId, message);
//                    break;
//                default:
//                    result = EtwProvider.EventWriteLogCritical((uint)traceId, message);
//                    break;
//            }
//            return result;
//        }

//        // Token: 0x0600006D RID: 109 RVA: 0x00003417 File Offset: 0x00001617
//        internal static bool ShouldTrace(TraceEventType eventType)
//        {
//            return Tracer.TraceSource.Switch.ShouldTrace(eventType) || EtwProvider.IsLoggingEnabled(Tracer.TraceEventTypeToLevel(eventType));
//        }

//        // Token: 0x0600006E RID: 110 RVA: 0x00003438 File Offset: 0x00001638
//        internal static bool TraceEvent(TraceEventType eventType, TraceId traceId)
//        {
//            return Tracer.TraceEvent(eventType, traceId, string.Empty);
//        }

//        // Token: 0x0600006F RID: 111 RVA: 0x00003448 File Offset: 0x00001648
//        internal static bool TraceEvent(TraceEventType eventType, TraceId traceId, string message)
//        {
//            bool flag = true;
//            Tracer.TraceSource.TraceEvent(eventType, (int)traceId, message);
//            Tracer.TraceSource.Flush();
//            if (message == null || message.Length <= 1754)
//            {
//                flag = Tracer.WriteEtwEvent(eventType, traceId, message);
//            }
//            else
//            {
//                int num = message.Length / 1754;
//                int num2 = 0;
//                for (int i = 0; i < num; i++)
//                {
//                    flag &= Tracer.WriteEtwEvent(eventType, traceId, message.Substring(num2, 1754));
//                    num2 += 1754;
//                }
//                if (num2 < message.Length)
//                {
//                    flag &= Tracer.WriteEtwEvent(eventType, traceId, message.Substring(num2, message.Length - num2));
//                }
//            }
//            return flag;
//        }

//        // Token: 0x06000070 RID: 112 RVA: 0x000034E5 File Offset: 0x000016E5
//        internal static bool TraceEvent(TraceEventType eventType, TraceId traceId, string format, params object[] args)
//        {
//            return Tracer.TraceEvent(eventType, traceId, string.Format(CultureInfo.CurrentCulture, format, args));
//        }

//        // Token: 0x06000071 RID: 113 RVA: 0x000034FA File Offset: 0x000016FA
//        internal static bool TraceException(TraceId traceId, Exception exception)
//        {
//            return Tracer.TraceEvent(TraceEventType.Error, traceId, string.Format(CultureInfo.CurrentCulture, "{0}", exception));
//        }

//        // Token: 0x06000072 RID: 114 RVA: 0x00003513 File Offset: 0x00001713
//        internal static bool TraceException(TraceId traceId, Exception exception, string message)
//        {
//            return Tracer.TraceEvent(TraceEventType.Error, traceId, string.Format(CultureInfo.CurrentCulture, "{0} {1}", message, exception));
//        }

//        // Token: 0x06000073 RID: 115 RVA: 0x00003530 File Offset: 0x00001730
//        internal static bool TraceException(TraceEventType eventType, TraceId traceId, Exception exception, string message)
//        {
//            StringBuilder stringBuilder = new StringBuilder(message);
//            if (exception != null)
//            {
//                stringBuilder.AppendLine();
//                foreach (object obj in exception.Data)
//                {
//                    DictionaryEntry dictionaryEntry = (DictionaryEntry)obj;
//                    stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0}: {1}", dictionaryEntry.Key, dictionaryEntry.Value);
//                    stringBuilder.AppendLine();
//                }
//            }
//            return Tracer.TraceEvent(eventType, traceId, string.Format(CultureInfo.CurrentCulture, "{0} Exception: {1}", stringBuilder.ToString(), exception));
//        }

//        // Token: 0x06000074 RID: 116 RVA: 0x000035D8 File Offset: 0x000017D8
//        internal static bool TraceHResult(TraceId traceId, int hr)
//        {
//            return Tracer.TraceHResult(traceId, hr, string.Empty);
//        }

//        // Token: 0x06000075 RID: 117 RVA: 0x000035E8 File Offset: 0x000017E8
//        internal static bool TraceHResult(TraceId traceId, int hr, string message)
//        {
//            Exception exceptionForHR = Marshal.GetExceptionForHR(hr);
//            return exceptionForHR != null && Tracer.TraceException(traceId, exceptionForHR, "HRESULT failure: " + message);
//        }

//        // Token: 0x06000076 RID: 118 RVA: 0x00003614 File Offset: 0x00001814
//        internal static bool DebugTraceEvent(TraceEventType eventType, TraceId traceId, string message)
//        {
//            return Tracer.TraceEvent(eventType, traceId, message);
//        }

//        // Token: 0x06000077 RID: 119 RVA: 0x0000362B File Offset: 0x0000182B
//        internal static bool AssertTraceEvent(bool condition, TraceEventType eventType, TraceId traceId, string message)
//        {
//            return condition || Tracer.DebugTraceEvent(eventType, traceId, message);
//        }

//        // Token: 0x06000078 RID: 120 RVA: 0x0000363C File Offset: 0x0000183C
//        internal static bool DebugTraceException(TraceEventType eventType, TraceId traceId, Exception exception, string message)
//        {
//            return Tracer.TraceException(eventType, traceId, exception, message);
//        }

//        // Token: 0x06000079 RID: 121 RVA: 0x00003654 File Offset: 0x00001854
//        internal static bool AssertTraceException(bool condition, TraceEventType eventType, TraceId traceId, Exception exception, string message)
//        {
//            return condition || Tracer.DebugTraceException(eventType, traceId, exception, message);
//        }

//        // Token: 0x04000097 RID: 151
//        private const string TraceSourceName = "Microsoft.Data.Tools.Diagnostics.Tracer";

//        // Token: 0x04000098 RID: 152
//        private const int MessageChunkLength = 1754;

//        // Token: 0x04000099 RID: 153
//        private static TraceSource _traceSource;
//    }
//    internal static class EtwProvider
//    {
//        // Token: 0x06000362 RID: 866 RVA: 0x0000D95C File Offset: 0x0000BB5C
//        private static bool GetIsEtwEnabled()
//        {
//            bool result;
//            try
//            {
//                result = (Environment.OSVersion.Version.Major > 5);
//            }
//            catch (InvalidOperationException)
//            {
//                result = false;
//            }
//            return result;
//        }

//        // Token: 0x06000363 RID: 867 RVA: 0x0000D994 File Offset: 0x0000BB94
//        public static bool IsEnabled()
//        {
//            return EtwProvider.EtwLoggingEnabled && EtwProvider.m_provider != null && EtwProvider.m_provider.IsEnabled();
//        }

//        // Token: 0x06000364 RID: 868 RVA: 0x0000D9B0 File Offset: 0x0000BBB0
//        public static bool IsEnabled(byte level, long keywords)
//        {
//            return EtwProvider.EtwLoggingEnabled && EtwProvider.m_provider != null && EtwProvider.m_provider.IsEnabled(level, keywords);
//        }

//        // Token: 0x06000365 RID: 869 RVA: 0x0000D9CE File Offset: 0x0000BBCE
//        public static bool IsModelStoreQueryExecutionTimesEnabled()
//        {
//            return EtwProvider.IsEnabled(EtwProvider.ModelStoreQueryExecutionTimes.Level, EtwProvider.ModelStoreQueryExecutionTimes.Keywords);
//        }

//        // Token: 0x06000366 RID: 870 RVA: 0x0000D9E9 File Offset: 0x0000BBE9
//        public static bool IsExecutePopulatorEnabled()
//        {
//            return EtwProvider.IsEnabled(EtwProvider.ExecutePopulator.Level, EtwProvider.ExecutePopulator.Keywords);
//        }

//        // Token: 0x06000367 RID: 871 RVA: 0x0000DA04 File Offset: 0x0000BC04
//        public static bool IsModelStoreFileSizeOnDisposeEnabled()
//        {
//            return EtwProvider.IsEnabled(EtwProvider.ModelStoreFileSizeOnDispose.Level, EtwProvider.ModelStoreFileSizeOnDispose.Keywords);
//        }

//        // Token: 0x06000368 RID: 872 RVA: 0x0000DA1F File Offset: 0x0000BC1F
//        public static bool IsSingleTaskProcessBatchEnabled()
//        {
//            return EtwProvider.IsEnabled(EtwProvider.SingleTaskProcessBatch.Level, EtwProvider.SingleTaskProcessBatch.Keywords);
//        }

//        // Token: 0x06000369 RID: 873 RVA: 0x0000DA3A File Offset: 0x0000BC3A
//        public static bool IsExecuteComposedPopulatorEnabled()
//        {
//            return EtwProvider.IsEnabled(EtwProvider.ExecuteComposedPopulator.Level, EtwProvider.ExecuteComposedPopulator.Keywords);
//        }

//        // Token: 0x0600036A RID: 874 RVA: 0x0000DA55 File Offset: 0x0000BC55
//        public static bool IsLoggingEnabled(WindowsEventTracingLevel level)
//        {
//            return EtwProvider.IsEnabled((byte)level, EtwProvider.LogCritical.Keywords);
//        }

//        // Token: 0x0600036C RID: 876 RVA: 0x0000E594 File Offset: 0x0000C794
//        public static bool EventWritePopulate(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.Populate, IsStart);
//        }

//        // Token: 0x0600036D RID: 877 RVA: 0x0000E5AF File Offset: 0x0000C7AF
//        public static bool EventWriteExecutePopulator(bool IsStart, string PopulatorName, int numberOfElements)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplatePopulatorMessage(ref EtwProvider.ExecutePopulator, IsStart, PopulatorName, numberOfElements);
//        }

//        // Token: 0x0600036E RID: 878 RVA: 0x0000E5CC File Offset: 0x0000C7CC
//        public static bool EventWriteExecuteComposedPopulator(bool IsStart, string PopulatorName, int numberOfElements)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplatePopulatorMessage(ref EtwProvider.ExecuteComposedPopulator, IsStart, PopulatorName, numberOfElements);
//        }

//        // Token: 0x0600036F RID: 879 RVA: 0x0000E5E9 File Offset: 0x0000C7E9
//        public static bool EventWriteDeleteElements()
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEventDescriptor(ref EtwProvider.DeleteElements);
//        }

//        // Token: 0x06000370 RID: 880 RVA: 0x0000E603 File Offset: 0x0000C803
//        public static bool EventWriteSchemaCompare(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.SchemaCompare, IsStart, EventContext);
//        }

//        // Token: 0x06000371 RID: 881 RVA: 0x0000E61F File Offset: 0x0000C81F
//        public static bool EventWriteSchemaCompareError(string message)
//        {
//            return EtwProvider.IsEnabled() && EtwProvider.m_provider.WriteEvent(ref EtwProvider.SchemaCompareError, message);
//        }

//        // Token: 0x06000372 RID: 882 RVA: 0x0000E63A File Offset: 0x0000C83A
//        public static bool EventWriteSqlEditorExecute(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.SqlEditorExecute, IsStart, EventContext);
//        }

//        // Token: 0x06000373 RID: 883 RVA: 0x0000E656 File Offset: 0x0000C856
//        public static bool EventWriteProjectLoad(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.ProjectLoad, IsStart, EventContext);
//        }

//        // Token: 0x06000374 RID: 884 RVA: 0x0000E672 File Offset: 0x0000C872
//        public static bool EventWriteProjectOpen(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.ProjectOpen, IsStart, EventContext);
//        }

//        // Token: 0x06000375 RID: 885 RVA: 0x0000E68E File Offset: 0x0000C88E
//        public static bool EventWriteProjectWizardImportSchemaFinish(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.ProjectWizardImportSchemaFinish, IsStart, EventContext);
//        }

//        // Token: 0x06000376 RID: 886 RVA: 0x0000E6AA File Offset: 0x0000C8AA
//        public static bool EventWriteProjectBuild(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.ProjectBuild, IsStart, EventContext);
//        }

//        // Token: 0x06000377 RID: 887 RVA: 0x0000E6C6 File Offset: 0x0000C8C6
//        public static bool EventWriteProjectBuildError(string message)
//        {
//            return EtwProvider.IsEnabled() && EtwProvider.m_provider.WriteEvent(ref EtwProvider.ProjectBuildError, message);
//        }

//        // Token: 0x06000378 RID: 888 RVA: 0x0000E6E1 File Offset: 0x0000C8E1
//        public static bool EventWriteProjectDeploy(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.ProjectDeploy, IsStart, EventContext);
//        }

//        // Token: 0x06000379 RID: 889 RVA: 0x0000E6FD File Offset: 0x0000C8FD
//        public static bool EventWriteDeploymentExecute(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.DeploymentExecute, IsStart, EventContext);
//        }

//        // Token: 0x0600037A RID: 890 RVA: 0x0000E719 File Offset: 0x0000C919
//        public static bool EventWriteDeploymentFailure(string message)
//        {
//            return EtwProvider.IsEnabled() && EtwProvider.m_provider.WriteEvent(ref EtwProvider.DeploymentFailure, message);
//        }

//        // Token: 0x0600037B RID: 891 RVA: 0x0000E734 File Offset: 0x0000C934
//        public static bool EventWriteDeploymentError(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.DeploymentError, IsStart, EventContext);
//        }

//        // Token: 0x0600037C RID: 892 RVA: 0x0000E750 File Offset: 0x0000C950
//        public static bool EventWriteDisplayAdapterSchemaObjectChangeDone(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.DisplayAdapterSchemaObjectChangeDone, IsStart, EventContext);
//        }

//        // Token: 0x0600037D RID: 893 RVA: 0x0000E76C File Offset: 0x0000C96C
//        public static bool EventWriteSchemaViewNodePopulationComplete(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.SchemaViewNodePopulationComplete, IsStart, EventContext);
//        }

//        // Token: 0x0600037E RID: 894 RVA: 0x0000E788 File Offset: 0x0000C988
//        public static bool EventWriteConnectionStringPersistedInRegistry(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.ConnectionStringPersistedInRegistry, IsStart, EventContext);
//        }

//        // Token: 0x0600037F RID: 895 RVA: 0x0000E7A4 File Offset: 0x0000C9A4
//        public static bool EventWriteDataSchemaModelRecycle(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.DataSchemaModelRecycle, IsStart, EventContext);
//        }

//        // Token: 0x06000380 RID: 896 RVA: 0x0000E7C0 File Offset: 0x0000C9C0
//        public static bool EventWriteModelStoreQueryExecutionTimes(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.ModelStoreQueryExecutionTimes, IsStart, EventContext);
//        }

//        // Token: 0x06000381 RID: 897 RVA: 0x0000E7DC File Offset: 0x0000C9DC
//        public static bool EventWriteModelStoreFileSizeOnDispose(string message)
//        {
//            return EtwProvider.IsEnabled() && EtwProvider.m_provider.WriteEvent(ref EtwProvider.ModelStoreFileSizeOnDispose, message);
//        }

//        // Token: 0x06000382 RID: 898 RVA: 0x0000E7F7 File Offset: 0x0000C9F7
//        public static bool EventWriteReverseEngineerPopulateAll(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.ReverseEngineerPopulateAll, IsStart, EventContext);
//        }

//        // Token: 0x06000383 RID: 899 RVA: 0x0000E813 File Offset: 0x0000CA13
//        public static bool EventWriteReverseEngineerPopulateSingle(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.ReverseEngineerPopulateSingle, IsStart, EventContext);
//        }

//        // Token: 0x06000384 RID: 900 RVA: 0x0000E82F File Offset: 0x0000CA2F
//        public static bool EventWriteReverseEngineerPopulateChildren(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.ReverseEngineerPopulateChildren, IsStart, EventContext);
//        }

//        // Token: 0x06000385 RID: 901 RVA: 0x0000E84B File Offset: 0x0000CA4B
//        public static bool EventWriteReverseEngineerExecuteReader(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.ReverseEngineerExecuteReader, IsStart, EventContext);
//        }

//        // Token: 0x06000386 RID: 902 RVA: 0x0000E867 File Offset: 0x0000CA67
//        public static bool EventWriteReverseEngineerElementsPopulated(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.ReverseEngineerElementsPopulated, IsStart, EventContext);
//        }

//        // Token: 0x06000387 RID: 903 RVA: 0x0000E883 File Offset: 0x0000CA83
//        public static bool EventWriteImportSchema(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.ImportSchema, IsStart, EventContext);
//        }

//        // Token: 0x06000388 RID: 904 RVA: 0x0000E89F File Offset: 0x0000CA9F
//        public static bool EventWriteImportSchemaFinish(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.ImportSchemaFinish, IsStart, EventContext);
//        }

//        // Token: 0x06000389 RID: 905 RVA: 0x0000E8BB File Offset: 0x0000CABB
//        public static bool EventWriteImportSchemaFinishError(string message)
//        {
//            return EtwProvider.IsEnabled() && EtwProvider.m_provider.WriteEvent(ref EtwProvider.ImportSchemaFinishError, message);
//        }

//        // Token: 0x0600038A RID: 906 RVA: 0x0000E8D6 File Offset: 0x0000CAD6
//        public static bool EventWriteImportSchemaGenerateAllScripts(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.ImportSchemaGenerateAllScripts, IsStart, EventContext);
//        }

//        // Token: 0x0600038B RID: 907 RVA: 0x0000E8F2 File Offset: 0x0000CAF2
//        public static bool EventWriteImportSchemaGenerateSingleScript(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.ImportSchemaGenerateSingleScript, IsStart, EventContext);
//        }

//        // Token: 0x0600038C RID: 908 RVA: 0x0000E90E File Offset: 0x0000CB0E
//        public static bool EventWriteImportSchemaAddAllScriptsToProject(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.ImportSchemaAddAllScriptsToProject, IsStart, EventContext);
//        }

//        // Token: 0x0600038D RID: 909 RVA: 0x0000E92A File Offset: 0x0000CB2A
//        public static bool EventWriteImportSchemaAddSingleScriptToProject(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.ImportSchemaAddSingleScriptToProject, IsStart, EventContext);
//        }

//        // Token: 0x0600038E RID: 910 RVA: 0x0000E946 File Offset: 0x0000CB46
//        public static bool EventWriteImportSchemaGenerateProjectMapForType(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.ImportSchemaGenerateProjectMapForType, IsStart, EventContext);
//        }

//        // Token: 0x0600038F RID: 911 RVA: 0x0000E962 File Offset: 0x0000CB62
//        public static bool EventWriteImportSchemaGenerateProjectMapForElement(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.ImportSchemaGenerateProjectMapForElement, IsStart, EventContext);
//        }

//        // Token: 0x06000390 RID: 912 RVA: 0x0000E97E File Offset: 0x0000CB7E
//        public static bool EventWriteImportSchemaAddScriptsToProjectForType(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.ImportSchemaAddScriptsToProjectForType, IsStart, EventContext);
//        }

//        // Token: 0x06000391 RID: 913 RVA: 0x0000E99A File Offset: 0x0000CB9A
//        public static bool EventWriteImportScript(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.ImportScript, IsStart, EventContext);
//        }

//        // Token: 0x06000392 RID: 914 RVA: 0x0000E9B6 File Offset: 0x0000CBB6
//        public static bool EventWriteModelProcessingTasks(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.ModelProcessingTasks, IsStart, EventContext);
//        }

//        // Token: 0x06000393 RID: 915 RVA: 0x0000E9D2 File Offset: 0x0000CBD2
//        public static bool EventWriteResolveAll(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.ResolveAll, IsStart);
//        }

//        // Token: 0x06000394 RID: 916 RVA: 0x0000E9ED File Offset: 0x0000CBED
//        public static bool EventWriteResolveBatch(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.ResolveBatch, IsStart);
//        }

//        // Token: 0x06000395 RID: 917 RVA: 0x0000EA08 File Offset: 0x0000CC08
//        public static bool EventWriteSingleTaskProcessAll(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.SingleTaskProcessAll, IsStart, EventContext);
//        }

//        // Token: 0x06000396 RID: 918 RVA: 0x0000EA24 File Offset: 0x0000CC24
//        public static bool EventWriteSingleTaskProcessBatch(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.SingleTaskProcessBatch, IsStart, EventContext);
//        }

//        // Token: 0x06000397 RID: 919 RVA: 0x0000EA40 File Offset: 0x0000CC40
//        public static bool EventWriteModelBuilder(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.ModelBuilder, IsStart, EventContext);
//        }

//        // Token: 0x06000398 RID: 920 RVA: 0x0000EA5C File Offset: 0x0000CC5C
//        public static bool EventWriteParseAndInterpret(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.ParseAndInterpret, IsStart, EventContext);
//        }

//        // Token: 0x06000399 RID: 921 RVA: 0x0000EA78 File Offset: 0x0000CC78
//        public static bool EventWriteParse(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.Parse, IsStart, EventContext);
//        }

//        // Token: 0x0600039A RID: 922 RVA: 0x0000EA94 File Offset: 0x0000CC94
//        public static bool EventWriteInterpret(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.Interpret, IsStart, EventContext);
//        }

//        // Token: 0x0600039B RID: 923 RVA: 0x0000EAB0 File Offset: 0x0000CCB0
//        public static bool EventWriteInterpretError(string message)
//        {
//            return EtwProvider.IsEnabled() && EtwProvider.m_provider.WriteEvent(ref EtwProvider.InterpretError, message);
//        }

//        // Token: 0x0600039C RID: 924 RVA: 0x0000EACB File Offset: 0x0000CCCB
//        public static bool EventWriteInterpretCritical(string message)
//        {
//            return EtwProvider.IsEnabled() && EtwProvider.m_provider.WriteEvent(ref EtwProvider.InterpretCritical, message);
//        }

//        // Token: 0x0600039D RID: 925 RVA: 0x0000EAE6 File Offset: 0x0000CCE6
//        public static bool EventWriteAnalyzeIdentifiedElement(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.AnalyzeIdentifiedElement, IsStart);
//        }

//        // Token: 0x0600039E RID: 926 RVA: 0x0000EB01 File Offset: 0x0000CD01
//        public static bool EventWriteAnalyzeIdentifiedRelationship(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.AnalyzeIdentifiedRelationship, IsStart);
//        }

//        // Token: 0x0600039F RID: 927 RVA: 0x0000EB1C File Offset: 0x0000CD1C
//        public static bool EventWriteAnalyzeIdentifiedRelationshipError()
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEventDescriptor(ref EtwProvider.AnalyzeIdentifiedRelationshipError);
//        }

//        // Token: 0x060003A0 RID: 928 RVA: 0x0000EB36 File Offset: 0x0000CD36
//        public static bool EventWriteAnalyzeIdentifiedSupportingStatement(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.AnalyzeIdentifiedSupportingStatement, IsStart);
//        }

//        // Token: 0x060003A1 RID: 929 RVA: 0x0000EB51 File Offset: 0x0000CD51
//        public static bool EventWriteAnalyzeIdentifiedAmbiguousRelationship(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.AnalyzeIdentifiedAmbiguousRelationship, IsStart);
//        }

//        // Token: 0x060003A2 RID: 930 RVA: 0x0000EB6C File Offset: 0x0000CD6C
//        public static bool EventWriteAnalyzeIdentifiedAmbiguousRelationshipError()
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEventDescriptor(ref EtwProvider.AnalyzeIdentifiedAmbiguousRelationshipError);
//        }

//        // Token: 0x060003A3 RID: 931 RVA: 0x0000EB86 File Offset: 0x0000CD86
//        public static bool EventWriteSemanticVerification(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.SemanticVerification, IsStart, EventContext);
//        }

//        // Token: 0x060003A4 RID: 932 RVA: 0x0000EBA2 File Offset: 0x0000CDA2
//        public static bool EventWriteSerializationWriteStore(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.SerializationWriteStore, IsStart, EventContext);
//        }

//        // Token: 0x060003A5 RID: 933 RVA: 0x0000EBBE File Offset: 0x0000CDBE
//        public static bool EventWriteSerializationWriteElement(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.SerializationWriteElement, IsStart, EventContext);
//        }

//        // Token: 0x060003A6 RID: 934 RVA: 0x0000EBDA File Offset: 0x0000CDDA
//        public static bool EventWriteSerializationWriteProperties(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.SerializationWriteProperties, IsStart, EventContext);
//        }

//        // Token: 0x060003A7 RID: 935 RVA: 0x0000EBF6 File Offset: 0x0000CDF6
//        public static bool EventWriteSerializationWriteRelationship(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.SerializationWriteRelationship, IsStart, EventContext);
//        }

//        // Token: 0x060003A8 RID: 936 RVA: 0x0000EC12 File Offset: 0x0000CE12
//        public static bool EventWriteSerializationWriteAnnotations(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.SerializationWriteAnnotations, IsStart, EventContext);
//        }

//        // Token: 0x060003A9 RID: 937 RVA: 0x0000EC2E File Offset: 0x0000CE2E
//        public static bool EventWriteSerializationWriteStoreAnnotations(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.SerializationWriteStoreAnnotations, IsStart, EventContext);
//        }

//        // Token: 0x060003AA RID: 938 RVA: 0x0000EC4A File Offset: 0x0000CE4A
//        public static bool EventWriteSerializationWriteRelationshipEntryPeer(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.SerializationWriteRelationshipEntryPeer, IsStart, EventContext);
//        }

//        // Token: 0x060003AB RID: 939 RVA: 0x0000EC66 File Offset: 0x0000CE66
//        public static bool EventWriteSerializationWriteRelationshipEntryComposing(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.SerializationWriteRelationshipEntryComposing, IsStart, EventContext);
//        }

//        // Token: 0x060003AC RID: 940 RVA: 0x0000EC82 File Offset: 0x0000CE82
//        public static bool EventWriteSerializationGetDisambiguatorMap(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.SerializationGetDisambiguatorMap, IsStart, EventContext);
//        }

//        // Token: 0x060003AD RID: 941 RVA: 0x0000EC9E File Offset: 0x0000CE9E
//        public static bool EventWriteSerializationGetRootElements(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.SerializationGetRootElements, IsStart, EventContext);
//        }

//        // Token: 0x060003AE RID: 942 RVA: 0x0000ECBA File Offset: 0x0000CEBA
//        public static bool EventWriteSerializationGetExternalSourceExternalName(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.SerializationGetExternalSourceExternalName, IsStart, EventContext);
//        }

//        // Token: 0x060003AF RID: 943 RVA: 0x0000ECD6 File Offset: 0x0000CED6
//        public static bool EventWriteDataSchemaModelSerialization(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.DataSchemaModelSerialization, IsStart);
//        }

//        // Token: 0x060003B0 RID: 944 RVA: 0x0000ECF1 File Offset: 0x0000CEF1
//        public static bool EventWriteDataSchemaModelDeserialization(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.DataSchemaModelDeserialization, IsStart, EventContext);
//        }

//        // Token: 0x060003B1 RID: 945 RVA: 0x0000ED0D File Offset: 0x0000CF0D
//        public static bool EventWriteDataSchemaModelDeserializationError(string message)
//        {
//            return EtwProvider.IsEnabled() && EtwProvider.m_provider.WriteEvent(ref EtwProvider.DataSchemaModelDeserializationError, message);
//        }

//        // Token: 0x060003B2 RID: 946 RVA: 0x0000ED28 File Offset: 0x0000CF28
//        public static bool EventWriteModelCompare(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.ModelCompare, IsStart, EventContext);
//        }

//        // Token: 0x060003B3 RID: 947 RVA: 0x0000ED44 File Offset: 0x0000CF44
//        public static bool EventWriteCommit(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.Commit, IsStart);
//        }

//        // Token: 0x060003B4 RID: 948 RVA: 0x0000ED5F File Offset: 0x0000CF5F
//        public static bool EventWriteBuildDac(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.BuildDac, IsStart);
//        }

//        // Token: 0x060003B5 RID: 949 RVA: 0x0000ED7A File Offset: 0x0000CF7A
//        public static bool EventWriteRunValidationRule(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.RunValidationRule, IsStart, EventContext);
//        }

//        // Token: 0x060003B6 RID: 950 RVA: 0x0000ED96 File Offset: 0x0000CF96
//        public static bool EventWriteRunExtendedValidation(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.RunExtendedValidation, IsStart);
//        }

//        // Token: 0x060003B7 RID: 951 RVA: 0x0000EDB1 File Offset: 0x0000CFB1
//        public static bool EventWriteLogCritical(uint traceId, string message)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateLoggingMessage(ref EtwProvider.LogCritical, traceId, message);
//        }

//        // Token: 0x060003B8 RID: 952 RVA: 0x0000EDCD File Offset: 0x0000CFCD
//        public static bool EventWriteLogError(uint traceId, string message)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateLoggingMessage(ref EtwProvider.LogError, traceId, message);
//        }

//        // Token: 0x060003B9 RID: 953 RVA: 0x0000EDE9 File Offset: 0x0000CFE9
//        public static bool EventWriteLogWarning(uint traceId, string message)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateLoggingMessage(ref EtwProvider.LogWarning, traceId, message);
//        }

//        // Token: 0x060003BA RID: 954 RVA: 0x0000EE05 File Offset: 0x0000D005
//        public static bool EventWriteLogInformational(uint traceId, string message)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateLoggingMessage(ref EtwProvider.LogInformational, traceId, message);
//        }

//        // Token: 0x060003BB RID: 955 RVA: 0x0000EE21 File Offset: 0x0000D021
//        public static bool EventWriteLogVerbose(uint traceId, string message)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateLoggingMessage(ref EtwProvider.LogVerbose, traceId, message);
//        }

//        // Token: 0x060003BC RID: 956 RVA: 0x0000EE3D File Offset: 0x0000D03D
//        public static bool EventWriteTableDesignerUpdateContextView(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.TableDesignerUpdateContextView, IsStart);
//        }

//        // Token: 0x060003BD RID: 957 RVA: 0x0000EE58 File Offset: 0x0000D058
//        public static bool EventWriteTableDesignerAddNewTable(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.TableDesignerAddNewTable, IsStart);
//        }

//        // Token: 0x060003BE RID: 958 RVA: 0x0000EE73 File Offset: 0x0000D073
//        public static bool EventWriteTableDesignerOpenTable(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.TableDesignerOpenTable, IsStart);
//        }

//        // Token: 0x060003BF RID: 959 RVA: 0x0000EE8E File Offset: 0x0000D08E
//        public static bool EventWriteTableDesignerSpecifyTableProperties(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.TableDesignerSpecifyTableProperties, IsStart);
//        }

//        // Token: 0x060003C0 RID: 960 RVA: 0x0000EEA9 File Offset: 0x0000D0A9
//        public static bool EventWriteTableDesignerAddColumns(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.TableDesignerAddColumns, IsStart);
//        }

//        // Token: 0x060003C1 RID: 961 RVA: 0x0000EEC4 File Offset: 0x0000D0C4
//        public static bool EventWriteTableDesignerAddObjectFromCtxPane(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.TableDesignerAddObjectFromCtxPane, IsStart);
//        }

//        // Token: 0x060003C2 RID: 962 RVA: 0x0000EEDF File Offset: 0x0000D0DF
//        public static bool EventWriteTableDesignerRefactorRename(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.TableDesignerRefactorRename, IsStart);
//        }

//        // Token: 0x060003C3 RID: 963 RVA: 0x0000EEFA File Offset: 0x0000D0FA
//        public static bool EventWriteTableDesignerDeleteColumns(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.TableDesignerDeleteColumns, IsStart, EventContext);
//        }

//        // Token: 0x060003C4 RID: 964 RVA: 0x0000EF16 File Offset: 0x0000D116
//        public static bool EventWriteSchemaCompareDataPopulationJob(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.SchemaCompareDataPopulationJob, IsStart, EventContext);
//        }

//        // Token: 0x060003C5 RID: 965 RVA: 0x0000EF32 File Offset: 0x0000D132
//        public static bool EventWriteSchemaCompareDataPopulationCancel(string message)
//        {
//            return EtwProvider.IsEnabled() && EtwProvider.m_provider.WriteEvent(ref EtwProvider.SchemaCompareDataPopulationCancel, message);
//        }

//        // Token: 0x060003C6 RID: 966 RVA: 0x0000EF4D File Offset: 0x0000D14D
//        public static bool EventWriteSchemaCompareScriptPopulationJob(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.SchemaCompareScriptPopulationJob, IsStart, EventContext);
//        }

//        // Token: 0x060003C7 RID: 967 RVA: 0x0000EF69 File Offset: 0x0000D169
//        public static bool EventWriteSchemaCompareScriptPopulationCancel(string message)
//        {
//            return EtwProvider.IsEnabled() && EtwProvider.m_provider.WriteEvent(ref EtwProvider.SchemaCompareScriptPopulationCancel, message);
//        }

//        // Token: 0x060003C8 RID: 968 RVA: 0x0000EF84 File Offset: 0x0000D184
//        public static bool EventWriteSchemaCompareGetAndResolveDataSchemaModel(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.SchemaCompareGetAndResolveDataSchemaModel, IsStart, EventContext);
//        }

//        // Token: 0x060003C9 RID: 969 RVA: 0x0000EFA0 File Offset: 0x0000D1A0
//        public static bool EventWriteSchemaCompareModelCompare(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.SchemaCompareModelCompare, IsStart, EventContext);
//        }

//        // Token: 0x060003CA RID: 970 RVA: 0x0000EFBC File Offset: 0x0000D1BC
//        public static bool EventWriteSchemaCompareGenerateVisual(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.SchemaCompareGenerateVisual, IsStart, EventContext);
//        }

//        // Token: 0x060003CB RID: 971 RVA: 0x0000EFD8 File Offset: 0x0000D1D8
//        public static bool EventWriteSchemaCompareUpdateTargetJob(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.SchemaCompareUpdateTargetJob, IsStart, EventContext);
//        }

//        // Token: 0x060003CC RID: 972 RVA: 0x0000EFF4 File Offset: 0x0000D1F4
//        public static bool EventWriteSchemaCompareUpdateTargetCancel(string message)
//        {
//            return EtwProvider.IsEnabled() && EtwProvider.m_provider.WriteEvent(ref EtwProvider.SchemaCompareUpdateTargetCancel, message);
//        }

//        // Token: 0x060003CD RID: 973 RVA: 0x0000F00F File Offset: 0x0000D20F
//        public static bool EventWriteProjectSystemSnapshot(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.ProjectSystemSnapshot, IsStart);
//        }

//        // Token: 0x060003CE RID: 974 RVA: 0x0000F02A File Offset: 0x0000D22A
//        public static bool EventWriteProjectSystemSnapshotBuildFailed()
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEventDescriptor(ref EtwProvider.ProjectSystemSnapshotBuildFailed);
//        }

//        // Token: 0x060003CF RID: 975 RVA: 0x0000F044 File Offset: 0x0000D244
//        public static bool EventWriteProjectSystemPublishing(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.ProjectSystemPublishing, IsStart);
//        }

//        // Token: 0x060003D0 RID: 976 RVA: 0x0000F05F File Offset: 0x0000D25F
//        public static bool EventWriteProjectSystemPublishCreateDeploymentPlan(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.ProjectSystemPublishCreateDeploymentPlan, IsStart);
//        }

//        // Token: 0x060003D1 RID: 977 RVA: 0x0000F07A File Offset: 0x0000D27A
//        public static bool EventWriteProjectSystemPublishCreatePublishScripts(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.ProjectSystemPublishCreatePublishScripts, IsStart);
//        }

//        // Token: 0x060003D2 RID: 978 RVA: 0x0000F095 File Offset: 0x0000D295
//        public static bool EventWriteProjectSystemPublishShowScript(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.ProjectSystemPublishShowScript, IsStart);
//        }

//        // Token: 0x060003D3 RID: 979 RVA: 0x0000F0B0 File Offset: 0x0000D2B0
//        public static bool EventWriteProjectSystemPublishExecuteScript(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.ProjectSystemPublishExecuteScript, IsStart);
//        }

//        // Token: 0x060003D4 RID: 980 RVA: 0x0000F0CB File Offset: 0x0000D2CB
//        public static bool EventWriteProjectSystemPublishResults(string message)
//        {
//            return EtwProvider.IsEnabled() && EtwProvider.m_provider.WriteEvent(ref EtwProvider.ProjectSystemPublishResults, message);
//        }

//        // Token: 0x060003D5 RID: 981 RVA: 0x0000F0E6 File Offset: 0x0000D2E6
//        public static bool EventWriteQueryResultExecuteQuery(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.QueryResultExecuteQuery, IsStart);
//        }

//        // Token: 0x060003D6 RID: 982 RVA: 0x0000F101 File Offset: 0x0000D301
//        public static bool EventWriteQueryResultCreateScript(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.QueryResultCreateScript, IsStart);
//        }

//        // Token: 0x060003D7 RID: 983 RVA: 0x0000F11C File Offset: 0x0000D31C
//        public static bool EventWriteQueryResultsLoaded()
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEventDescriptor(ref EtwProvider.QueryResultsLoaded);
//        }

//        // Token: 0x060003D8 RID: 984 RVA: 0x0000F136 File Offset: 0x0000D336
//        public static bool EventWriteProjectSystemImportSnapshot(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.ProjectSystemImportSnapshot, IsStart);
//        }

//        // Token: 0x060003D9 RID: 985 RVA: 0x0000F151 File Offset: 0x0000D351
//        public static bool EventWriteFileOpen(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.FileOpen, IsStart, EventContext);
//        }

//        // Token: 0x060003DA RID: 986 RVA: 0x0000F16D File Offset: 0x0000D36D
//        public static bool EventWriteLoadTSqlDocData(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.LoadTSqlDocData, IsStart, EventContext);
//        }

//        // Token: 0x060003DB RID: 987 RVA: 0x0000F189 File Offset: 0x0000D389
//        public static bool EventWriteTSqlEditorFrameCreate(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.TSqlEditorFrameCreate, IsStart, EventContext);
//        }

//        // Token: 0x060003DC RID: 988 RVA: 0x0000F1A5 File Offset: 0x0000D3A5
//        public static bool EventWriteTSqlEditorActivate(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.TSqlEditorActivate, IsStart, EventContext);
//        }

//        // Token: 0x060003DD RID: 989 RVA: 0x0000F1C1 File Offset: 0x0000D3C1
//        public static bool EventWriteTSqlEditorTabSwitch(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.TSqlEditorTabSwitch, IsStart, EventContext);
//        }

//        // Token: 0x060003DE RID: 990 RVA: 0x0000F1DD File Offset: 0x0000D3DD
//        public static bool EventWriteTSqlEditorLaunch(bool IsStart, string EventContext)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateGenericBeginEndMessage(ref EtwProvider.TSqlEditorLaunch, IsStart, EventContext);
//        }

//        // Token: 0x060003DF RID: 991 RVA: 0x0000F1F9 File Offset: 0x0000D3F9
//        public static bool EventWriteTSqlOnlineEditorDocumentLoad(string message)
//        {
//            return EtwProvider.IsEnabled() && EtwProvider.m_provider.WriteEvent(ref EtwProvider.TSqlOnlineEditorDocumentLoad, message);
//        }

//        // Token: 0x060003E0 RID: 992 RVA: 0x0000F214 File Offset: 0x0000D414
//        public static bool EventWriteServerExplorerServerPropertiesRetrieved(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.ServerExplorerServerPropertiesRetrieved, IsStart);
//        }

//        // Token: 0x060003E1 RID: 993 RVA: 0x0000F22F File Offset: 0x0000D42F
//        public static bool EventWriteGotoDefinition(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.GotoDefinition, IsStart);
//        }

//        // Token: 0x060003E2 RID: 994 RVA: 0x0000F24A File Offset: 0x0000D44A
//        public static bool EventWriteFindAllReferences(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.FindAllReferences, IsStart);
//        }

//        // Token: 0x060003E3 RID: 995 RVA: 0x0000F265 File Offset: 0x0000D465
//        public static bool EventWriteRefactor(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.Refactor, IsStart);
//        }

//        // Token: 0x060003E4 RID: 996 RVA: 0x0000F280 File Offset: 0x0000D480
//        public static bool EventWriteRefactorContributeChanges(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.RefactorContributeChanges, IsStart);
//        }

//        // Token: 0x060003E5 RID: 997 RVA: 0x0000F29B File Offset: 0x0000D49B
//        public static bool EventWriteRefactorApplyChanges(bool IsStart)
//        {
//            return !EtwProvider.IsEnabled() || EtwProvider.m_provider.TemplateEmptyBeginEndMessage(ref EtwProvider.RefactorApplyChanges, IsStart);
//        }

//        // Token: 0x040003E0 RID: 992
//        private static bool EtwLoggingEnabled = EtwProvider.GetIsEtwEnabled();

//        // Token: 0x040003E1 RID: 993
//        internal static EventProviderVersionTwo m_provider = new EventProviderVersionTwo(new Guid("79f618ad-4b02-4d46-a525-f5a93c551ddd"));

//        // Token: 0x040003E2 RID: 994
//        private static Guid ReverseEngineerId = new Guid("0c270cbc-3e29-449d-8a86-97dd3efb008b");

//        // Token: 0x040003E3 RID: 995
//        private static Guid CoreModelId = new Guid("ee88552a-6edb-48c3-8730-47e357196a05");

//        // Token: 0x040003E4 RID: 996
//        private static EventDescriptor Populate = new EventDescriptor(0, 1, 0, 1, 11, 1, 5L);

//        // Token: 0x040003E5 RID: 997
//        private static EventDescriptor ExecutePopulator = new EventDescriptor(1, 1, 0, 4, 11, 1, 5L);

//        // Token: 0x040003E6 RID: 998
//        private static EventDescriptor ExecuteComposedPopulator = new EventDescriptor(2, 1, 0, 5, 11, 1, 5L);

//        // Token: 0x040003E7 RID: 999
//        private static EventDescriptor DeleteElements = new EventDescriptor(3, 1, 0, 5, 11, 1, 5L);

//        // Token: 0x040003E8 RID: 1000
//        private static EventDescriptor SchemaCompare = new EventDescriptor(4, 1, 0, 4, 0, 3, 513L);

//        // Token: 0x040003E9 RID: 1001
//        private static EventDescriptor SchemaCompareError = new EventDescriptor(69, 1, 0, 2, 0, 3, 513L);

//        // Token: 0x040003EA RID: 1002
//        private static EventDescriptor SqlEditorExecute = new EventDescriptor(5, 1, 0, 4, 0, 3, 513L);

//        // Token: 0x040003EB RID: 1003
//        private static EventDescriptor ProjectLoad = new EventDescriptor(6, 1, 0, 4, 0, 3, 513L);

//        // Token: 0x040003EC RID: 1004
//        private static EventDescriptor ProjectOpen = new EventDescriptor(7, 1, 0, 4, 0, 3, 513L);

//        // Token: 0x040003ED RID: 1005
//        private static EventDescriptor ProjectWizardImportSchemaFinish = new EventDescriptor(8, 1, 0, 4, 0, 3, 513L);

//        // Token: 0x040003EE RID: 1006
//        private static EventDescriptor ProjectBuild = new EventDescriptor(9, 1, 0, 1, 0, 3, 513L);

//        // Token: 0x040003EF RID: 1007
//        private static EventDescriptor ProjectBuildError = new EventDescriptor(70, 1, 0, 2, 0, 3, 513L);

//        // Token: 0x040003F0 RID: 1008
//        private static EventDescriptor ProjectDeploy = new EventDescriptor(10, 1, 0, 1, 0, 3, 513L);

//        // Token: 0x040003F1 RID: 1009
//        private static EventDescriptor DeploymentExecute = new EventDescriptor(11, 1, 0, 4, 0, 3, 513L);

//        // Token: 0x040003F2 RID: 1010
//        private static EventDescriptor DeploymentFailure = new EventDescriptor(12, 1, 0, 4, 0, 3, 513L);

//        // Token: 0x040003F3 RID: 1011
//        private static EventDescriptor DeploymentError = new EventDescriptor(13, 1, 0, 2, 0, 3, 513L);

//        // Token: 0x040003F4 RID: 1012
//        private static EventDescriptor DisplayAdapterSchemaObjectChangeDone = new EventDescriptor(14, 1, 0, 4, 0, 4, 1025L);

//        // Token: 0x040003F5 RID: 1013
//        private static EventDescriptor SchemaViewNodePopulationComplete = new EventDescriptor(15, 1, 0, 4, 0, 4, 1025L);

//        // Token: 0x040003F6 RID: 1014
//        private static EventDescriptor ConnectionStringPersistedInRegistry = new EventDescriptor(16, 1, 0, 4, 0, 4, 1025L);

//        // Token: 0x040003F7 RID: 1015
//        private static EventDescriptor DataSchemaModelRecycle = new EventDescriptor(17, 1, 0, 4, 0, 4, 1025L);

//        // Token: 0x040003F8 RID: 1016
//        private static EventDescriptor ModelStoreQueryExecutionTimes = new EventDescriptor(18, 1, 0, 4, 0, 4, 1025L);

//        // Token: 0x040003F9 RID: 1017
//        private static EventDescriptor ModelStoreFileSizeOnDispose = new EventDescriptor(19, 1, 0, 4, 0, 4, 1025L);

//        // Token: 0x040003FA RID: 1018
//        private static EventDescriptor ReverseEngineerPopulateAll = new EventDescriptor(20, 1, 0, 4, 0, 1, 5L);

//        // Token: 0x040003FB RID: 1019
//        private static EventDescriptor ReverseEngineerPopulateSingle = new EventDescriptor(21, 1, 0, 4, 0, 1, 5L);

//        // Token: 0x040003FC RID: 1020
//        private static EventDescriptor ReverseEngineerPopulateChildren = new EventDescriptor(22, 1, 0, 4, 0, 1, 5L);

//        // Token: 0x040003FD RID: 1021
//        private static EventDescriptor ReverseEngineerExecuteReader = new EventDescriptor(23, 1, 0, 4, 0, 1, 5L);

//        // Token: 0x040003FE RID: 1022
//        private static EventDescriptor ReverseEngineerElementsPopulated = new EventDescriptor(24, 1, 0, 4, 0, 1, 5L);

//        // Token: 0x040003FF RID: 1023
//        private static EventDescriptor ImportSchema = new EventDescriptor(25, 1, 0, 4, 0, 5, 9L);

//        // Token: 0x04000400 RID: 1024
//        private static EventDescriptor ImportSchemaFinish = new EventDescriptor(26, 1, 0, 4, 0, 5, 9L);

//        // Token: 0x04000401 RID: 1025
//        private static EventDescriptor ImportSchemaFinishError = new EventDescriptor(68, 1, 0, 4, 0, 5, 9L);

//        // Token: 0x04000402 RID: 1026
//        private static EventDescriptor ImportSchemaGenerateAllScripts = new EventDescriptor(27, 1, 0, 4, 0, 5, 9L);

//        // Token: 0x04000403 RID: 1027
//        private static EventDescriptor ImportSchemaGenerateSingleScript = new EventDescriptor(28, 1, 0, 4, 0, 5, 9L);

//        // Token: 0x04000404 RID: 1028
//        private static EventDescriptor ImportSchemaAddAllScriptsToProject = new EventDescriptor(29, 1, 0, 4, 0, 5, 9L);

//        // Token: 0x04000405 RID: 1029
//        private static EventDescriptor ImportSchemaAddSingleScriptToProject = new EventDescriptor(30, 1, 0, 4, 0, 5, 9L);

//        // Token: 0x04000406 RID: 1030
//        private static EventDescriptor ImportSchemaGenerateProjectMapForType = new EventDescriptor(31, 1, 0, 4, 0, 5, 9L);

//        // Token: 0x04000407 RID: 1031
//        private static EventDescriptor ImportSchemaGenerateProjectMapForElement = new EventDescriptor(32, 1, 0, 4, 0, 5, 9L);

//        // Token: 0x04000408 RID: 1032
//        private static EventDescriptor ImportSchemaAddScriptsToProjectForType = new EventDescriptor(33, 1, 0, 4, 0, 5, 9L);

//        // Token: 0x04000409 RID: 1033
//        private static EventDescriptor ImportScript = new EventDescriptor(34, 1, 0, 4, 0, 6, 17L);

//        // Token: 0x0400040A RID: 1034
//        private static EventDescriptor ModelProcessingTasks = new EventDescriptor(35, 1, 0, 4, 0, 7, 33L);

//        // Token: 0x0400040B RID: 1035
//        private static EventDescriptor ResolveAll = new EventDescriptor(36, 1, 0, 1, 0, 7, 33L);

//        // Token: 0x0400040C RID: 1036
//        private static EventDescriptor ResolveBatch = new EventDescriptor(37, 1, 0, 4, 0, 7, 33L);

//        // Token: 0x0400040D RID: 1037
//        private static EventDescriptor SingleTaskProcessAll = new EventDescriptor(38, 1, 0, 4, 0, 7, 33L);

//        // Token: 0x0400040E RID: 1038
//        private static EventDescriptor SingleTaskProcessBatch = new EventDescriptor(39, 1, 0, 4, 0, 7, 33L);

//        // Token: 0x0400040F RID: 1039
//        private static EventDescriptor ModelBuilder = new EventDescriptor(40, 1, 0, 4, 0, 8, 65L);

//        // Token: 0x04000410 RID: 1040
//        private static EventDescriptor ParseAndInterpret = new EventDescriptor(41, 1, 0, 4, 0, 8, 65L);

//        // Token: 0x04000411 RID: 1041
//        private static EventDescriptor Parse = new EventDescriptor(42, 1, 0, 4, 0, 8, 65L);

//        // Token: 0x04000412 RID: 1042
//        private static EventDescriptor Interpret = new EventDescriptor(43, 1, 0, 4, 0, 8, 65L);

//        // Token: 0x04000413 RID: 1043
//        private static EventDescriptor InterpretError = new EventDescriptor(44, 1, 0, 2, 0, 8, 65L);

//        // Token: 0x04000414 RID: 1044
//        private static EventDescriptor InterpretCritical = new EventDescriptor(45, 1, 0, 1, 0, 8, 65L);

//        // Token: 0x04000415 RID: 1045
//        private static EventDescriptor AnalyzeIdentifiedElement = new EventDescriptor(46, 1, 0, 4, 0, 8, 65L);

//        // Token: 0x04000416 RID: 1046
//        private static EventDescriptor AnalyzeIdentifiedRelationship = new EventDescriptor(47, 1, 0, 4, 0, 8, 65L);

//        // Token: 0x04000417 RID: 1047
//        private static EventDescriptor AnalyzeIdentifiedRelationshipError = new EventDescriptor(48, 1, 0, 2, 0, 8, 65L);

//        // Token: 0x04000418 RID: 1048
//        private static EventDescriptor AnalyzeIdentifiedSupportingStatement = new EventDescriptor(49, 1, 0, 4, 0, 8, 65L);

//        // Token: 0x04000419 RID: 1049
//        private static EventDescriptor AnalyzeIdentifiedAmbiguousRelationship = new EventDescriptor(50, 1, 0, 4, 0, 8, 65L);

//        // Token: 0x0400041A RID: 1050
//        private static EventDescriptor AnalyzeIdentifiedAmbiguousRelationshipError = new EventDescriptor(51, 1, 0, 2, 0, 8, 65L);

//        // Token: 0x0400041B RID: 1051
//        private static EventDescriptor SemanticVerification = new EventDescriptor(52, 1, 0, 1, 0, 8, 65L);

//        // Token: 0x0400041C RID: 1052
//        private static EventDescriptor SerializationWriteStore = new EventDescriptor(53, 1, 0, 4, 0, 9, 129L);

//        // Token: 0x0400041D RID: 1053
//        private static EventDescriptor SerializationWriteElement = new EventDescriptor(54, 1, 0, 4, 0, 9, 129L);

//        // Token: 0x0400041E RID: 1054
//        private static EventDescriptor SerializationWriteProperties = new EventDescriptor(55, 1, 0, 4, 0, 9, 129L);

//        // Token: 0x0400041F RID: 1055
//        private static EventDescriptor SerializationWriteRelationship = new EventDescriptor(56, 1, 0, 4, 0, 9, 129L);

//        // Token: 0x04000420 RID: 1056
//        private static EventDescriptor SerializationWriteAnnotations = new EventDescriptor(57, 1, 0, 4, 0, 9, 129L);

//        // Token: 0x04000421 RID: 1057
//        private static EventDescriptor SerializationWriteStoreAnnotations = new EventDescriptor(58, 1, 0, 4, 0, 9, 129L);

//        // Token: 0x04000422 RID: 1058
//        private static EventDescriptor SerializationWriteRelationshipEntryPeer = new EventDescriptor(59, 1, 0, 4, 0, 9, 129L);

//        // Token: 0x04000423 RID: 1059
//        private static EventDescriptor SerializationWriteRelationshipEntryComposing = new EventDescriptor(60, 1, 0, 4, 0, 9, 129L);

//        // Token: 0x04000424 RID: 1060
//        private static EventDescriptor SerializationGetDisambiguatorMap = new EventDescriptor(61, 1, 0, 4, 0, 9, 129L);

//        // Token: 0x04000425 RID: 1061
//        private static EventDescriptor SerializationGetRootElements = new EventDescriptor(62, 1, 0, 4, 0, 9, 129L);

//        // Token: 0x04000426 RID: 1062
//        private static EventDescriptor SerializationGetExternalSourceExternalName = new EventDescriptor(63, 1, 0, 4, 0, 9, 129L);

//        // Token: 0x04000427 RID: 1063
//        private static EventDescriptor DataSchemaModelSerialization = new EventDescriptor(64, 1, 0, 4, 0, 9, 129L);

//        // Token: 0x04000428 RID: 1064
//        private static EventDescriptor DataSchemaModelDeserialization = new EventDescriptor(65, 1, 0, 4, 0, 9, 129L);

//        // Token: 0x04000429 RID: 1065
//        private static EventDescriptor DataSchemaModelDeserializationError = new EventDescriptor(66, 1, 0, 2, 0, 9, 129L);

//        // Token: 0x0400042A RID: 1066
//        private static EventDescriptor ModelCompare = new EventDescriptor(67, 1, 0, 4, 0, 10, 257L);

//        // Token: 0x0400042B RID: 1067
//        private static EventDescriptor Commit = new EventDescriptor(87, 1, 0, 4, 0, 0, 1L);

//        // Token: 0x0400042C RID: 1068
//        private static EventDescriptor BuildDac = new EventDescriptor(76, 1, 0, 1, 0, 0, 1L);

//        // Token: 0x0400042D RID: 1069
//        private static EventDescriptor RunValidationRule = new EventDescriptor(77, 1, 0, 4, 0, 8, 65L);

//        // Token: 0x0400042E RID: 1070
//        private static EventDescriptor RunExtendedValidation = new EventDescriptor(78, 1, 0, 4, 0, 3, 513L);

//        // Token: 0x0400042F RID: 1071
//        private static EventDescriptor LogCritical = new EventDescriptor(71, 1, 0, 1, 0, 11, 2048L);

//        // Token: 0x04000430 RID: 1072
//        private static EventDescriptor LogError = new EventDescriptor(72, 1, 0, 2, 0, 11, 2048L);

//        // Token: 0x04000431 RID: 1073
//        private static EventDescriptor LogWarning = new EventDescriptor(73, 1, 0, 3, 0, 11, 2048L);

//        // Token: 0x04000432 RID: 1074
//        private static EventDescriptor LogInformational = new EventDescriptor(74, 1, 0, 4, 0, 11, 2048L);

//        // Token: 0x04000433 RID: 1075
//        private static EventDescriptor LogVerbose = new EventDescriptor(75, 1, 0, 5, 0, 11, 2048L);

//        // Token: 0x04000434 RID: 1076
//        private static EventDescriptor TableDesignerUpdateContextView = new EventDescriptor(79, 1, 16, 4, 0, 0, -9223372036854771711L);

//        // Token: 0x04000435 RID: 1077
//        private static EventDescriptor TableDesignerAddNewTable = new EventDescriptor(80, 1, 0, 4, 0, 0, 4097L);

//        // Token: 0x04000436 RID: 1078
//        private static EventDescriptor TableDesignerOpenTable = new EventDescriptor(81, 1, 0, 4, 0, 0, 4097L);

//        // Token: 0x04000437 RID: 1079
//        private static EventDescriptor TableDesignerSpecifyTableProperties = new EventDescriptor(82, 1, 0, 4, 0, 0, 4097L);

//        // Token: 0x04000438 RID: 1080
//        private static EventDescriptor TableDesignerAddColumns = new EventDescriptor(83, 1, 0, 1, 0, 0, 4097L);

//        // Token: 0x04000439 RID: 1081
//        private static EventDescriptor TableDesignerAddObjectFromCtxPane = new EventDescriptor(84, 1, 0, 4, 0, 0, 4097L);

//        // Token: 0x0400043A RID: 1082
//        private static EventDescriptor TableDesignerRefactorRename = new EventDescriptor(85, 1, 0, 4, 0, 0, 4097L);

//        // Token: 0x0400043B RID: 1083
//        private static EventDescriptor TableDesignerDeleteColumns = new EventDescriptor(86, 1, 0, 4, 0, 0, 4097L);

//        // Token: 0x0400043C RID: 1084
//        private static EventDescriptor SchemaCompareDataPopulationJob = new EventDescriptor(88, 1, 0, 4, 0, 0, 16385L);

//        // Token: 0x0400043D RID: 1085
//        private static EventDescriptor SchemaCompareDataPopulationCancel = new EventDescriptor(89, 1, 0, 4, 0, 0, 16385L);

//        // Token: 0x0400043E RID: 1086
//        private static EventDescriptor SchemaCompareScriptPopulationJob = new EventDescriptor(90, 1, 0, 4, 0, 0, 16385L);

//        // Token: 0x0400043F RID: 1087
//        private static EventDescriptor SchemaCompareScriptPopulationCancel = new EventDescriptor(91, 1, 0, 4, 0, 0, 16385L);

//        // Token: 0x04000440 RID: 1088
//        private static EventDescriptor SchemaCompareGetAndResolveDataSchemaModel = new EventDescriptor(92, 1, 0, 4, 0, 0, 16385L);

//        // Token: 0x04000441 RID: 1089
//        private static EventDescriptor SchemaCompareModelCompare = new EventDescriptor(93, 1, 0, 4, 0, 0, 16385L);

//        // Token: 0x04000442 RID: 1090
//        private static EventDescriptor SchemaCompareGenerateVisual = new EventDescriptor(94, 1, 0, 4, 0, 0, 16385L);

//        // Token: 0x04000443 RID: 1091
//        private static EventDescriptor SchemaCompareUpdateTargetJob = new EventDescriptor(95, 1, 0, 4, 0, 0, 16385L);

//        // Token: 0x04000444 RID: 1092
//        private static EventDescriptor SchemaCompareUpdateTargetCancel = new EventDescriptor(96, 1, 0, 4, 0, 0, 16385L);

//        // Token: 0x04000445 RID: 1093
//        private static EventDescriptor ProjectSystemSnapshot = new EventDescriptor(120, 1, 0, 4, 0, 0, 513L);

//        // Token: 0x04000446 RID: 1094
//        private static EventDescriptor ProjectSystemSnapshotBuildFailed = new EventDescriptor(121, 1, 0, 4, 0, 0, 513L);

//        // Token: 0x04000447 RID: 1095
//        private static EventDescriptor ProjectSystemPublishing = new EventDescriptor(122, 1, 0, 4, 0, 0, 513L);

//        // Token: 0x04000448 RID: 1096
//        private static EventDescriptor ProjectSystemPublishCreateDeploymentPlan = new EventDescriptor(123, 1, 0, 4, 0, 0, 513L);

//        // Token: 0x04000449 RID: 1097
//        private static EventDescriptor ProjectSystemPublishCreatePublishScripts = new EventDescriptor(124, 1, 0, 4, 0, 0, 513L);

//        // Token: 0x0400044A RID: 1098
//        private static EventDescriptor ProjectSystemPublishShowScript = new EventDescriptor(125, 1, 0, 4, 0, 0, 513L);

//        // Token: 0x0400044B RID: 1099
//        private static EventDescriptor ProjectSystemPublishExecuteScript = new EventDescriptor(126, 1, 0, 4, 0, 0, 513L);

//        // Token: 0x0400044C RID: 1100
//        private static EventDescriptor ProjectSystemPublishResults = new EventDescriptor(127, 1, 0, 4, 0, 0, 513L);

//        // Token: 0x0400044D RID: 1101
//        private static EventDescriptor QueryResultExecuteQuery = new EventDescriptor(128, 1, 16, 4, 0, 0, -9223372036854710271L);

//        // Token: 0x0400044E RID: 1102
//        private static EventDescriptor QueryResultCreateScript = new EventDescriptor(129, 1, 16, 4, 0, 0, -9223372036854710271L);

//        // Token: 0x0400044F RID: 1103
//        private static EventDescriptor QueryResultsLoaded = new EventDescriptor(130, 1, 16, 4, 0, 0, -9223372036854710271L);

//        // Token: 0x04000450 RID: 1104
//        private static EventDescriptor ProjectSystemImportSnapshot = new EventDescriptor(131, 1, 16, 4, 0, 0, -9223372036854644735L);

//        // Token: 0x04000451 RID: 1105
//        private static EventDescriptor FileOpen = new EventDescriptor(150, 1, 0, 4, 0, 3, 33281L);

//        // Token: 0x04000452 RID: 1106
//        private static EventDescriptor LoadTSqlDocData = new EventDescriptor(151, 1, 0, 4, 0, 12, 32769L);

//        // Token: 0x04000453 RID: 1107
//        private static EventDescriptor TSqlEditorFrameCreate = new EventDescriptor(152, 1, 0, 1, 0, 12, 32769L);

//        // Token: 0x04000454 RID: 1108
//        private static EventDescriptor TSqlEditorActivate = new EventDescriptor(153, 1, 0, 1, 0, 12, 32769L);

//        // Token: 0x04000455 RID: 1109
//        private static EventDescriptor TSqlEditorTabSwitch = new EventDescriptor(154, 1, 0, 1, 0, 12, 32769L);

//        // Token: 0x04000456 RID: 1110
//        private static EventDescriptor TSqlEditorLaunch = new EventDescriptor(155, 1, 0, 1, 0, 12, 32769L);

//        // Token: 0x04000457 RID: 1111
//        private static EventDescriptor TSqlOnlineEditorDocumentLoad = new EventDescriptor(156, 1, 16, 4, 0, 0, -9223372036854743039L);

//        // Token: 0x04000458 RID: 1112
//        private static EventDescriptor ServerExplorerServerPropertiesRetrieved = new EventDescriptor(200, 1, 0, 4, 0, 0, 513L);

//        // Token: 0x04000459 RID: 1113
//        private static EventDescriptor GotoDefinition = new EventDescriptor(300, 1, 16, 1, 0, 0, -9223372036854513151L);

//        // Token: 0x0400045A RID: 1114
//        private static EventDescriptor FindAllReferences = new EventDescriptor(301, 1, 16, 1, 0, 0, -9223372036854513151L);

//        // Token: 0x0400045B RID: 1115
//        private static EventDescriptor Refactor = new EventDescriptor(302, 1, 16, 1, 0, 0, -9223372036854513151L);

//        // Token: 0x0400045C RID: 1116
//        private static EventDescriptor RefactorContributeChanges = new EventDescriptor(303, 1, 16, 1, 0, 0, -9223372036854513151L);

//        // Token: 0x0400045D RID: 1117
//        private static EventDescriptor RefactorApplyChanges = new EventDescriptor(304, 1, 16, 1, 0, 0, -9223372036854513151L);
//    }
//    internal class EventProviderVersionTwo : EventProvider
//    {
//        // Token: 0x060003E6 RID: 998 RVA: 0x0000F2B6 File Offset: 0x0000D4B6
//        internal EventProviderVersionTwo(Guid id) : base(id)
//        {
//        }

//        // Token: 0x060003E7 RID: 999 RVA: 0x0000F2C0 File Offset: 0x0000D4C0
//        internal unsafe bool TemplateEmptyBeginEndMessage(ref EventDescriptor eventDescriptor, bool IsStart)
//        {
//            int num = 1;
//            bool result = true;
//            if (base.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
//            {
//                byte* ptr = stackalloc byte[(UIntPtr)(sizeof(EventProviderVersionTwo.EventData) * num)];
//                EventProviderVersionTwo.EventData* ptr2 = (EventProviderVersionTwo.EventData*)ptr;
//                int num2 = IsStart ? 1 : 0;
//                ptr2->DataPointer = &num2;
//                ptr2->Size = 4u;
//                result = base.WriteEvent(ref eventDescriptor, num, (IntPtr)((void*)ptr));
//            }
//            return result;
//        }

//        // Token: 0x060003E8 RID: 1000 RVA: 0x0000F320 File Offset: 0x0000D520
//        internal unsafe bool TemplatePopulatorMessage(ref EventDescriptor eventDescriptor, bool IsStart, string PopulatorName, int numberOfElements)
//        {
//            int num = 3;
//            bool result = true;
//            if (base.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
//            {
//                byte* ptr = stackalloc byte[(UIntPtr)(sizeof(EventProviderVersionTwo.EventData) * num)];
//                EventProviderVersionTwo.EventData* ptr2 = (EventProviderVersionTwo.EventData*)ptr;
//                int num2 = IsStart ? 1 : 0;
//                ptr2->DataPointer = &num2;
//                ptr2->Size = 4u;
//                ptr2[1].Size = (uint)(((PopulatorName ?? string.Empty).Length + 1) * 2);
//                ptr2[2].DataPointer = &numberOfElements;
//                ptr2[2].Size = 4u;
//                string text = PopulatorName ?? string.Empty;
//                fixed (char* ptr3 = text)
//                {
//                    ptr2[1].DataPointer = ptr3;
//                    result = base.WriteEvent(ref eventDescriptor, num, (IntPtr)((void*)ptr));
//                }
//            }
//            return result;
//        }

//        // Token: 0x060003E9 RID: 1001 RVA: 0x0000F3F6 File Offset: 0x0000D5F6
//        internal bool TemplateEventDescriptor(ref EventDescriptor eventDescriptor)
//        {
//            return !base.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords) || base.WriteEvent(ref eventDescriptor, 0, IntPtr.Zero);
//        }

//        // Token: 0x060003EA RID: 1002 RVA: 0x0000F41C File Offset: 0x0000D61C
//        internal unsafe bool TemplateGenericBeginEndMessage(ref EventDescriptor eventDescriptor, bool IsStart, string EventContext)
//        {
//            int num = 2;
//            bool result = true;
//            if (base.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
//            {
//                byte* ptr = stackalloc byte[(UIntPtr)(sizeof(EventProviderVersionTwo.EventData) * num)];
//                EventProviderVersionTwo.EventData* ptr2 = (EventProviderVersionTwo.EventData*)ptr;
//                int num2 = IsStart ? 1 : 0;
//                ptr2->DataPointer = &num2;
//                ptr2->Size = 4u;
//                ptr2[1].Size = (uint)(((EventContext ?? string.Empty).Length + 1) * 2);
//                string text = EventContext ?? string.Empty;
//                fixed (char* ptr3 = text)
//                {
//                    ptr2[1].DataPointer = ptr3;
//                    result = base.WriteEvent(ref eventDescriptor, num, (IntPtr)((void*)ptr));
//                }
//            }
//            return result;
//        }

//        // Token: 0x060003EB RID: 1003 RVA: 0x0000F4D0 File Offset: 0x0000D6D0
//        internal unsafe bool TemplateLoggingMessage(ref EventDescriptor eventDescriptor, uint traceId, string message)
//        {
//            int num = 2;
//            bool result = true;
//            if (base.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
//            {
//                byte* ptr = stackalloc byte[(UIntPtr)(sizeof(EventProviderVersionTwo.EventData) * num)];
//                EventProviderVersionTwo.EventData* ptr2 = (EventProviderVersionTwo.EventData*)ptr;
//                ptr2->DataPointer = &traceId;
//                ptr2->Size = 4u;
//                ptr2[1].Size = (uint)(((message ?? string.Empty).Length + 1) * 2);
//                string text = message ?? string.Empty;
//                fixed (char* ptr3 = text)
//                {
//                    ptr2[1].DataPointer = ptr3;
//                    result = base.WriteEvent(ref eventDescriptor, num, (IntPtr)((void*)ptr));
//                }
//            }
//            return result;
//        }

//        // Token: 0x02000058 RID: 88
//        [StructLayout(LayoutKind.Explicit, Size = 16)]
//        private struct EventData
//        {
//            // Token: 0x0400045E RID: 1118
//            [FieldOffset(0)]
//            internal ulong DataPointer;

//            // Token: 0x0400045F RID: 1119
//            [FieldOffset(8)]
//            internal uint Size;

//            // Token: 0x04000460 RID: 1120
//            [FieldOffset(12)]
//            internal int Reserved;
//        }
//    }
//}