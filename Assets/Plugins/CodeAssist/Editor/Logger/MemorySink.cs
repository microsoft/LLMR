using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;


#nullable enable


namespace Meryel.UnityCodeAssist.Editor.Logger
{
    //**--
    // remove this in unity???
    // need to serialize/deserialize data to survive domain reload, which will effect performance
    // right now data is lost during domain reloads, which makes its function kinda useless
    // or maybe move it to a external process like com.unity.process-server
    public class MemorySink : ILogEventSink
    {
        readonly ConcurrentQueue<LogEvent> logs;
        readonly ConcurrentQueue<LogEvent[]> warningLogs;
        readonly ConcurrentQueue<LogEvent[]> errorLogs;

        const int logsLimit = 30;
        const int warningLimit = 5;
        const int errorLimit = 3;

        readonly string outputTemplate;

        public MemorySink(string outputTemplate)
        {
            this.outputTemplate = outputTemplate;

            logs = new ConcurrentQueue<LogEvent>();
            warningLogs = new ConcurrentQueue<LogEvent[]>();
            errorLogs = new ConcurrentQueue<LogEvent[]>();
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null)
                return;

            logs.Enqueue(logEvent);
            if (logs.Count > logsLimit)
                logs.TryDequeue(out _);

            if (logEvent.Level == LogEventLevel.Warning)
            {
                var warningAndLeadingLogs = logs.ToArray();
                warningLogs.Enqueue(warningAndLeadingLogs);
                if (warningLogs.Count > warningLimit)
                    warningLogs.TryDequeue(out _);
            }

            if (logEvent.Level == LogEventLevel.Error)
            {
                var errorAndLeadingLogs = logs.ToArray();
                errorLogs.Enqueue(errorAndLeadingLogs);
                if (errorLogs.Count > errorLimit)
                    errorLogs.TryDequeue(out _);
            }
        }

        public bool HasError => !errorLogs.IsEmpty;
        public bool HasWarning => !warningLogs.IsEmpty;
        public int ErrorCount => errorLogs.Count;
        public int WarningCount => warningLogs.Count;

        public string Export()
        {
            IFormatProvider? formatProvider = null;
            var formatter = new Serilog.Formatting.Display.MessageTemplateTextFormatter(
                outputTemplate, formatProvider);

            var result = string.Empty;

            using (var outputStream = new MemoryStream())
            {
                var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                using var output = new StreamWriter(outputStream, encoding);
                if (!errorLogs.IsEmpty)
                {
                    var errorArray = errorLogs.ToArray();
                    foreach (var error in errorArray)
                    {
                        foreach (var logEvent in error)
                        {
                            formatter.Format(logEvent, output);
                        }
                    }
                }

                if (!warningLogs.IsEmpty)
                {
                    var warningArray = warningLogs.ToArray();
                    foreach (var warning in warningArray)
                    {
                        foreach (var logEvent in warning)
                        {
                            formatter.Format(logEvent, output);
                        }
                    }
                }

                if (!logs.IsEmpty)
                {
                    var logArray = logs.ToArray();
                    foreach (var logEvent in logArray)
                    {
                        formatter.Format(logEvent, output);
                    }
                }

                output.Flush();

                outputStream.Seek(0, SeekOrigin.Begin);
                using var streamReader = new StreamReader(outputStream, encoding);
                result = streamReader.ReadToEnd();


            }

            return result;
        }


    }
}
