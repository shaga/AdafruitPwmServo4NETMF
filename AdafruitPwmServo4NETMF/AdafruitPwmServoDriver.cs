using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace AdafruitPwmServo4NETMF
{
    public class AdafruitPwmServoDriver
    {
        #region const value

        private const int I2CClockRate = 200;

        private const int I2CTimeout = 100;

        private const byte MinI2CAddr = 0x40;
        private const byte MaxI2CAddr = 0x79;

        #region PCA96854

        private const byte Pca9685SubAddr1 = 0x02;
        private const byte Pca9685SubAddr2 = 0x03;
        private const byte Pca9685SubAddr3 = 0x04;

        private const byte Pca9685Mode1 = 0x00;
        private const byte Pca9685Prescale = 0xFE;

        private const byte Led0OnL = 0x06;
        private const byte Led0OnH = 0x07;
        private const byte Led0OffL = 0x08;
        private const byte Led0OffH = 0x09;

        private const byte AllLedOnL = 0xFA;
        private const byte AllLedOnH = 0xFB;
        private const byte AllLedOffL = 0xFC;
        private const byte AllLedOffH = 0xFD;

        #endregion

        #endregion

        #region proprety

        #region static

        public static I2CDevice I2CDevice { get; set; }

        #endregion

        private I2CDevice.Configuration Configuration { get; set; }

        private I2CDevice.I2CTransaction[] ReadTransactions { get; set; }
        private I2CDevice.I2CTransaction[] WriteTransactions { get; set; }

        #endregion

        #region constractor

        public AdafruitPwmServoDriver(ushort addr = MinI2CAddr, I2CDevice i2CDevice = null)
        {
            Configuration = new I2CDevice.Configuration(addr, I2CClockRate);

            if (I2CDevice == null)
            {
                I2CDevice = i2CDevice ?? new I2CDevice(Configuration);
            }

            ReadTransactions = new I2CDevice.I2CTransaction[2];
            WriteTransactions = new I2CDevice.I2CTransaction[1];
        } 

        #endregion

        #region private method

        private void WriteByte(byte addr, byte data)
        {
            WriteTransactions[0] = I2CDevice.CreateWriteTransaction(new[] {addr, data});

            I2CDevice.Config = Configuration;
            I2CDevice.Execute(WriteTransactions, I2CTimeout);
        }

        private byte ReadByte(byte addr)
        {
            var buffer = new byte[1];
            ReadTransactions[0] = I2CDevice.CreateWriteTransaction(new[] {addr});
            ReadTransactions[1] = I2CDevice.CreateReadTransaction(buffer);

            I2CDevice.Config = Configuration;
            I2CDevice.Execute(ReadTransactions, I2CTimeout);

            return buffer[0];
        }

        #endregion

        #region public method

        public void Begin()
        {
            Reset();
        }

        public void Reset()
        {
            WriteByte(Pca9685Mode1, 0x00);
        }

        public void SetPwmFreq(float freq)
        {
            var prescaleval = 25000000F;
            prescaleval /= 4096;
            prescaleval /= freq;
            prescaleval -= 1;

            var prescale = (byte) (prescaleval + 0.5);

            var oldmode = ReadByte(Pca9685Mode1);
            var newmode = (byte) ((oldmode & 0x7f) | 0x10);

            WriteByte(Pca9685Mode1, newmode);
            WriteByte(Pca9685Prescale, prescale);
            WriteByte(Pca9685Mode1, oldmode);
            Thread.Sleep(5);
            WriteByte(Pca9685Mode1, (byte)(oldmode | 0xa1));
        }

        public void SetPwm(byte num, ushort on, ushort off)
        {
            WriteByte((byte) (Led0OnL + 4*num), (byte) (on & 0xff));
            WriteByte((byte) (Led0OnH + 4*num), (byte) (on >> 8));
            WriteByte((byte) (Led0OffL + 4*num), (byte) (off & 0xff));
            WriteByte((byte) (Led0OffH + 4*num), (byte) (off >> 8));
        }

        #endregion
    }
}
