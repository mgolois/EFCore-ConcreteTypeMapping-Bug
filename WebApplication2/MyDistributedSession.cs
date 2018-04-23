using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebApplication2
{
    public class MyDistributedSession : ISession
    {

        private static readonly RandomNumberGenerator CryptoRandom = RandomNumberGenerator.Create();
        private const int IdByteCount = 16;

        private const byte SerializationRevision = 2;
        private const int KeyLengthLimit = ushort.MaxValue;

        private readonly IDistributedCache _cache;
        private readonly string _sessionKey;
        private readonly TimeSpan _idleTimeout;
        private readonly TimeSpan _ioTimeout;
        private readonly Func<bool> _tryEstablishSession;
        private readonly ILogger _logger;
        private IDictionary<string, object> _store;
        private bool _isModified;
        private bool _loaded;
        private bool _isAvailable;
        private bool _isNewSessionKey;
        private string _sessionId;
        private byte[] _sessionIdBytes;

        public MyDistributedSession(
            IDistributedCache cache,
            string sessionKey,
            TimeSpan idleTimeout,
            TimeSpan ioTimeout,
            Func<bool> tryEstablishSession,
            ILoggerFactory loggerFactory,
            bool isNewSessionKey)
        {
            if (string.IsNullOrEmpty(sessionKey))
            {
                throw new ArgumentException(nameof(sessionKey));
            }

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _sessionKey = sessionKey;
            _idleTimeout = idleTimeout;
            _ioTimeout = ioTimeout;
            _tryEstablishSession = tryEstablishSession ?? throw new ArgumentNullException(nameof(tryEstablishSession));
            _store = new Dictionary<string, object>();
            _logger = loggerFactory.CreateLogger<DistributedSession>();
            _isNewSessionKey = isNewSessionKey;
        }

        public bool IsAvailable
        {
            get
            {
                Load();
                return _isAvailable;
            }
        }

        public string Id
        {
            get
            {
                Load();
                if (_sessionId == null)
                {
                    _sessionId = new Guid(IdBytes).ToString();
                }
                return _sessionId;
            }
        }

        private byte[] IdBytes
        {
            get
            {
                if (IsAvailable && _sessionIdBytes == null)
                {
                    _sessionIdBytes = new byte[IdByteCount];
                    CryptoRandom.GetBytes(_sessionIdBytes);
                }
                return _sessionIdBytes;
            }
        }

        public IEnumerable<string> Keys
        {
            get
            {
                Load();
                return _store.Keys;
            }
        }

        public bool TryGetObject<T>(string key, out T value)
        {
            Load();
            if (_store.TryGetValue(key, out object obj))
            {
                var jObj = (JObject)obj;
                value = jObj.ToObject<T>();
                return true;
            }
            value = default(T);
            return false;
        }

        public void Set(string key, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (IsAvailable)
            {

                if (key.Length > KeyLengthLimit)
                {
                    throw new ArgumentOutOfRangeException(nameof(key));
                }

                if (!_tryEstablishSession())
                {
                    throw new InvalidOperationException();
                }
                _isModified = true;
                _store[key] = value;
            }
        }

        public void Remove(string key)
        {
            Load();
            _isModified |= _store.Remove(key);
        }

        public void Clear()
        {
            Load();
            _isModified |= _store.Count > 0;
            _store.Clear();
        }

        private void Load()
        {
            if (!_loaded)
            {
                try
                {
                    var data = _cache.Get(_sessionKey);
                    if (data != null)
                    {
                        Deserialize(data);
                    }
                    else if (!_isNewSessionKey)
                    {
                        _logger.LogInformation($"Accessing expired session, Key:{_sessionKey}");
                    }
                    _isAvailable = true;
                }
                catch (Exception exception)
                {
                    _logger.LogError($"Session cache read exception, Key:{_sessionKey}", exception);

                    _isAvailable = false;
                    _sessionId = string.Empty;
                    _sessionIdBytes = null;
                    _store = new Dictionary<string, object>();
                }
                finally
                {
                    _loaded = true;
                }
            }
        }

        // This will throw if called directly and a failure occurs. The user is expected to handle the failures.
        public async Task LoadAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!_loaded)
            {
                using (var timeout = new CancellationTokenSource(_ioTimeout))
                {
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);
                    try
                    {
                        cts.Token.ThrowIfCancellationRequested();
                        var data = await _cache.GetAsync(_sessionKey, cts.Token);
                        if (data != null)
                        {
                            Deserialize(data);
                        }
                        else if (!_isNewSessionKey)
                        {
                            _logger.LogInformation($"Accessing expired session, Key:{_sessionKey}");
                        }
                    }
                    catch (OperationCanceledException oex)
                    {
                        if (timeout.Token.IsCancellationRequested)
                        {
                            _logger.LogWarning("Loading the session timed out.");
                            throw new OperationCanceledException("Timed out loading the session.", oex, timeout.Token);
                        }
                        throw;
                    }
                }
                _isAvailable = true;
                _loaded = true;
            }
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var timeout = new CancellationTokenSource(_ioTimeout))
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);
                if (_isModified)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        // This operation is only so we can log if the session already existed.
                        // Log and ignore failures.
                        try
                        {
                            cts.Token.ThrowIfCancellationRequested();
                            var data = await _cache.GetAsync(_sessionKey, cts.Token);
                            if (data == null)
                            {
                                _logger.LogInformation($"Session started; Key:{_sessionKey}, Id:{Id}");
                            }
                        }
                        catch (OperationCanceledException)
                        {
                        }
                        catch (Exception exception)
                        {
                            _logger.LogError($"Session cache read exception, Key:{_sessionKey}", exception);
                        }
                    }


                    Serialize(out byte[] json);

                    try
                    {
                        cts.Token.ThrowIfCancellationRequested();
                        await _cache.SetAsync(
                            _sessionKey,
                            json,
                            new DistributedCacheEntryOptions().SetSlidingExpiration(_idleTimeout),
                            cts.Token);
                        _isModified = false;
                        _logger.LogDebug($"Session stored; Key:{_sessionKey}, Id:{Id}, Count:{_store.Count}");
                    }
                    catch (OperationCanceledException oex)
                    {
                        if (timeout.Token.IsCancellationRequested)
                        {
                            _logger.LogWarning("Committing the session timed out.");
                            throw new OperationCanceledException("Timed out committing the session.", oex, timeout.Token);
                        }
                        throw;
                    }
                }
                else
                {
                    try
                    {
                        await _cache.RefreshAsync(_sessionKey, cts.Token);
                    }
                    catch (OperationCanceledException oex)
                    {
                        if (timeout.Token.IsCancellationRequested)
                        {
                            _logger.LogWarning("Refreshing the session timed out.");

                            throw new OperationCanceledException("Timed out refreshing the session.", oex, timeout.Token);
                        }
                        throw;
                    }
                }
            }
        }

        private void Serialize(out byte[] output)
        {
            var json = JsonConvert.SerializeObject(_store);
            //encrypt
            output = EncryptionHelper.EncryptStringToBytes(json);
        }

        private void Deserialize(byte[] content)
        {
          var json =  EncryptionHelper.DecryptStringFromBytes(content);
            _store = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _sessionId = new Guid(_sessionIdBytes).ToString();
                _logger.LogDebug($"Session loaded; Key:{_sessionKey}, Id:{_sessionId}, Count:{_store.Count}");
            }
        }



        public bool TryGetValue(string key, out byte[] value)
        {
            return TryGetValue(key, out value);
        }

        public void Set(string key, byte[] value)
        {
            Set(key, (object) value);
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            Load();
            var ret = _store.TryGetValue(key, out object val);
            value = (T)val;
            return ret;
        }
    }

}