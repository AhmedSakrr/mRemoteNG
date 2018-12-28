﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using mRemoteNG.Config.Serializers.Csv;
using mRemoteNG.Connection;
using mRemoteNG.Connection.Protocol;
using mRemoteNG.Connection.Protocol.Http;
using mRemoteNG.Connection.Protocol.ICA;
using mRemoteNG.Connection.Protocol.RDP;
using mRemoteNG.Connection.Protocol.VNC;
using mRemoteNG.Credential;
using mRemoteNG.Security;
using mRemoteNGTests.TestHelpers;
using NSubstitute;
using NUnit.Framework;

namespace mRemoteNGTests.Config.Serializers.ConnectionSerializers.Csv
{
    public class CsvConnectionsDeserializerMremotengFormatTests
    {
        private CsvConnectionsDeserializerMremotengFormat _deserializer;
        private CsvConnectionsSerializerMremotengFormat _serializer;

        [SetUp]
        public void Setup()
        {
            _deserializer = new CsvConnectionsDeserializerMremotengFormat();
            var credentialRepositoryList = Substitute.For<ICredentialRepositoryList>();
            _serializer = new CsvConnectionsSerializerMremotengFormat(new SaveFilter(), credentialRepositoryList);
        }

        [TestCaseSource(typeof(DeserializationTestSource), nameof(DeserializationTestSource.ConnectionPropertyTestCases))]
        public object ConnectionPropertiesDeserializedCorrectly(string propertyToCheck)
        {
            var csv = _serializer.Serialize(GetTestConnection());
            var deserializedConnections = _deserializer.Deserialize(csv);
            var connection = deserializedConnections.GetRecursiveChildList().FirstOrDefault();
            var propertyValue = typeof(ConnectionInfo).GetProperty(propertyToCheck)?.GetValue(connection);
            return propertyValue;
        }

        [TestCaseSource(typeof(DeserializationTestSource), nameof(DeserializationTestSource.InheritanceTestCases))]
        public object InheritancePropertiesDeserializedCorrectly(string propertyToCheck)
        {
            var csv = _serializer.Serialize(GetTestConnectionWithAllInherited());
            var deserializedConnections = _deserializer.Deserialize(csv);
            var connection = deserializedConnections.GetRecursiveChildList().FirstOrDefault();
            connection?.RemoveParent();
            var propertyValue = typeof(ConnectionInfoInheritance).GetProperty(propertyToCheck)?.GetValue(connection?.Inheritance);
            return propertyValue;
        }

        [Test]
        public void TreeStructureDeserializedCorrectly()
        {
            //Root
            // |- folder1
            // |   |- Con1
            // |- Con2
            var treeModel = new ConnectionTreeModelBuilder().Build();
            var csv = _serializer.Serialize(treeModel);
            var deserializedConnections = _deserializer.Deserialize(csv);
            var con1 = deserializedConnections.GetRecursiveChildList().First(info => info.Name == "Con1");
            var folder1 = deserializedConnections.GetRecursiveChildList().First(info => info.Name == "folder1");
            Assert.That(con1.Parent, Is.EqualTo(folder1));
        }

        internal static ConnectionInfo GetTestConnection()
        {
            return new ConnectionInfo
            {
                Name = "SomeName",
                Description = "SomeDescription",
                Icon = "SomeIcon",
                Panel = "SomePanel",
                Username = "SomeUsername",
                Password = "SomePassword",
                Domain = "SomeDomain",
                Hostname = "SomeHostname",
                PuttySession = "SomePuttySession",
                LoadBalanceInfo = "SomeLoadBalanceInfo",
                PreExtApp = "SomePreExtApp",
                PostExtApp = "SomePostExtApp",
                MacAddress = "SomeMacAddress",
                UserField = "SomeUserField",
                ExtApp = "SomeExtApp",
                VNCProxyUsername = "SomeVNCProxyUsername",
                VNCProxyPassword = "SomeVNCProxyPassword",
                RDGatewayUsername = "SomeRDGatewayUsername",
                RDGatewayPassword = "SomeRDGatewayPassword",
                RDGatewayDomain = "SomeRDGatewayDomain",
                VNCProxyIP = "SomeVNCProxyIP",
                RDGatewayHostname = "SomeRDGatewayHostname",
                Protocol = ProtocolType.ICA,
                Port = 999,
                UseConsoleSession = true,
                UseCredSsp = true,
                RenderingEngine = HTTPBase.RenderingEngine.Gecko,
                ICAEncryptionStrength = IcaProtocol.EncryptionStrength.Encr40Bit,
                RDPAuthenticationLevel = RdpProtocol.AuthenticationLevel.WarnOnFailedAuth,
                Colors = RdpProtocol.RDPColors.Colors16Bit,
                Resolution = RdpProtocol.RDPResolutions.Res1366x768,
                AutomaticResize = true,
                DisplayWallpaper = true,
                DisplayThemes = true,
                EnableFontSmoothing = true,
                EnableDesktopComposition = true,
                CacheBitmaps = true,
                RedirectDiskDrives = true,
                RedirectPorts = true,
                RedirectPrinters = true,
                RedirectSmartCards = true,
                RedirectSound = RdpProtocol.RDPSounds.LeaveAtRemoteComputer,
                RedirectKeys = true,
                VNCCompression = ProtocolVNC.Compression.Comp4,
                VNCEncoding = ProtocolVNC.Encoding.EncRRE,
                VNCAuthMode = ProtocolVNC.AuthMode.AuthVNC,
                VNCProxyType = ProtocolVNC.ProxyType.ProxySocks5,
                VNCProxyPort = 123,
                VNCColors = ProtocolVNC.Colors.Col8Bit,
                VNCSmartSizeMode = ProtocolVNC.SmartSizeMode.SmartSAspect,
                VNCViewOnly = true,
                RDGatewayUsageMethod = RdpProtocol.RDGatewayUsageMethod.Detect,
                RDGatewayUseConnectionCredentials = RdpProtocol.RDGatewayUseConnectionCredentials.SmartCard
            };
        }

        internal static ConnectionInfo GetTestConnectionWithAllInherited()
        {
            var connectionInfo = new ConnectionInfo();
            connectionInfo.Inheritance.TurnOnInheritanceCompletely();
            return connectionInfo;
        }

        private class DeserializationTestSource
        {
            public static IEnumerable ConnectionPropertyTestCases()
            {
                var properties = new ConnectionInfo().GetSerializableProperties();
                var testCases = new List<TestCaseData>();
                var testConnectionInfo = GetTestConnection();

                foreach (var property in properties)
                {
                    testCases.Add(
                        new TestCaseData(property.Name)
                        .Returns(property.GetValue(testConnectionInfo)));
                }

                return testCases;
            }

            public static IEnumerable InheritanceTestCases()
            {
                var properties = new ConnectionInfoInheritance(new object()).GetProperties();
                var testCases = new List<TestCaseData>();
                var testInheritance = GetTestConnectionWithAllInherited().Inheritance;

                foreach (var property in properties)
                {
                    testCases.Add(
                        new TestCaseData(property.Name)
                            .Returns(property.GetValue(testInheritance)));
                }

                return testCases;
            }
        }
    }
}
