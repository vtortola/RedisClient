using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using vtortola.Redis;

namespace UnitTest.RedisClient
{
    [TestClass]
    public class SocketWriterTests
    {
        [TestMethod]
        public void CanWrite()
        {
            using(var ms = new MemoryStream())
            using(var writer = new SocketWriter(ms, 8))
            using(var reader = new StreamReader(ms))
            {
                writer.Write("777\r\n".ToCharArray());
                writer.Write("888\r\n".ToCharArray());
                writer.Write("999\r\n".ToCharArray());
                writer.Write(new []{'x'});
                writer.Write("#".ToCharArray());
                writer.Write("\r\n".ToCharArray());

                writer.Flush();
                ms.Seek(0, SeekOrigin.Begin);

                Assert.AreEqual("777", reader.ReadLine());
                Assert.AreEqual("888", reader.ReadLine());
                Assert.AreEqual("999", reader.ReadLine());
                Assert.AreEqual("x#", reader.ReadLine());
            }
        }

        [TestMethod]
        public void CanReadUTF8Lines()
        {
            var str1 = "프로디지(The Prodigy)는 리엄 하울렛(Liam Howlett)이 주축이 되어 1990년 영국 에식스(Essex)에서 결성된 일렉트로닉 음악 그룹이다. 케미컬 브라더스(Chemical Brothers), 팻보이 슬림(Fatboy Slim), 크리스털 메소드(The Crystal Method) 등과 함께 빅비트(big beat) 일렉트로닉 댄스 음악의 선두주자로 꼽힌다. 현재까지 세계적으로 1600백만장 이상의 앨범 판매고를 올렸으며, 이는 일렉트로닉 음악 역사상 최고 음반 판매고이다.[1] 프로디지의 음악은 1990년대 초반의 레이브(rave), 하드코어 테크노(hardcore techno), 인더스트리얼(industrial)부터 이후의 얼터너티브 록(alternative rock)과 펑크(punk) 보컬을 가미한 빅비트까지 다양한 스타일이다.";
            var str2 = "현재, 멤버는 리엄 하울렛(Liam Howlett: 작곡/키보드), 키쓰 플린트(Keith Flint: 댄서/보컬), 막심(Maxim: MC/보컬)이다. 여성 댄서이자 보컬을 맡았던 샤키(Sharky)는 밴드 결성 초반에, 댄서이자 간혹 라이브에서 키보드를 맡았던 리로이 쏜힐(Leeroy Thornhill)은 2000년에 밴드에서 탈퇴하였다.";

            using (var ms = new MemoryStream())
            using (var writer = new SocketWriter(ms, 8))
            using (var reader = new StreamReader(ms))
            {
                writer.Write("This is line 1\r\n".ToCharArray());
                writer.Write((str1 + "\r\n").ToCharArray());
                writer.Write((str2 + "\r\n").ToCharArray());
                writer.Write("This is line 4\r\n".ToCharArray());

                writer.Flush();
                ms.Seek(0, SeekOrigin.Begin);

                Assert.AreEqual("This is line 1", reader.ReadLine());
                Assert.AreEqual(str1, reader.ReadLine());
                Assert.AreEqual(str2, reader.ReadLine());
                Assert.AreEqual("This is line 4", reader.ReadLine());
            }
        }
    }
}
