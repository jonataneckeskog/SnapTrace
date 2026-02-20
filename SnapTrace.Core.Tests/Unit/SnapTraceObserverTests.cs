using SnapTrace.Core.Configuration;
using SnapTrace.Core.Runtime;
using System.Reflection;

namespace SnapTrace.Core.Tests.Unit
{
    public class SnapTraceObserverTests
    {
        private static void ResetObserver()
        {
            var isInitializedIntField = typeof(SnapTraceObserver).GetField("_isInitializedInt", BindingFlags.NonPublic | BindingFlags.Static)!;
            isInitializedIntField.SetValue(null, 0);

            var bufferField = typeof(SnapTraceObserver).GetField("_buffer", BindingFlags.NonPublic | BindingFlags.Static);
            bufferField?.SetValue(null, null);

            var optionsField = typeof(SnapTraceObserver).GetField("_options", BindingFlags.NonPublic | BindingFlags.Static);
            optionsField?.SetValue(null, null);

            var serializerField = typeof(SnapTraceObserver).GetField("_snapSerializer", BindingFlags.NonPublic | BindingFlags.Static);
            serializerField?.SetValue(null, null);
        }

        [Fact]
        public void Initialize_ShouldSetOptionsAndBuffer()
        {
            // Arrange
            ResetObserver();
            var output = new List<string>();
            Action<string> outputAction = (s) => output.Add(s);
            var options = new SnapOptions(10, true, outputAction);

            // Act
            SnapTraceObserver.Initialize(options);

            // Assert
            var bufferField = typeof(SnapTraceObserver).GetField("_buffer", BindingFlags.NonPublic | BindingFlags.Static);
            var buffer = bufferField?.GetValue(null) as RingBuffer<SnapEntry>;
            Assert.NotNull(buffer);
            Assert.Equal(10, buffer.Capacity);

            var optionsField = typeof(SnapTraceObserver).GetField("_options", BindingFlags.NonPublic | BindingFlags.Static);
            var storedOptions = optionsField?.GetValue(null) as SnapOptions?;
            Assert.True(storedOptions.HasValue);
            Assert.Equal(options, storedOptions.Value);
        }

        [Fact]
        public void Initialize_ShouldBeCalledOnlyOnce()
        {
            // Arrange
            ResetObserver();
            var options1 = new SnapOptions(10, true, (s) => { });
            var options2 = new SnapOptions(20, false, (s) => { });

            // Act
            SnapTraceObserver.Initialize(options1);
            SnapTraceObserver.Initialize(options2);

            // Assert
            var bufferField = typeof(SnapTraceObserver).GetField("_buffer", BindingFlags.NonPublic | BindingFlags.Static);
            var buffer = bufferField?.GetValue(null) as RingBuffer<SnapEntry>;
            Assert.Equal(10, buffer?.Capacity); // Should remain from the first call
        }

        [Fact]
        public void Record_ShouldNotAddEntry_WhenNotInitialized()
        {
            // Arrange
            ResetObserver();
            var recordMethod = typeof(SnapTraceObserver).GetMethod("Record", BindingFlags.NonPublic | BindingFlags.Static);
            var entry = new SnapEntry("Test", null, null, SnapStatus.Call);

            // Act
            recordMethod?.Invoke(null, [entry]);

            // Assert
            var bufferField = typeof(SnapTraceObserver).GetField("_buffer", BindingFlags.NonPublic | BindingFlags.Static);
            var buffer = bufferField?.GetValue(null) as RingBuffer<SnapEntry>;
            Assert.Null(buffer);
        }

        [Fact]
        public void Record_ShouldAddEntry_WhenInitialized()
        {
            // Arrange
            ResetObserver();
            var options = new SnapOptions(10, true, (s) => { });
            SnapTraceObserver.Initialize(options);
            var recordMethod = typeof(SnapTraceObserver).GetMethod("Record", BindingFlags.NonPublic | BindingFlags.Static);
            var entry = new SnapEntry("Test", null, null, SnapStatus.Call);

            // Act
            recordMethod?.Invoke(null, [entry]);

            // Assert
            var bufferField = typeof(SnapTraceObserver).GetField("_buffer", BindingFlags.NonPublic | BindingFlags.Static);
            var buffer = bufferField?.GetValue(null) as RingBuffer<SnapEntry>;
            Assert.Equal(1, buffer?.Count);
        }
    }
}
