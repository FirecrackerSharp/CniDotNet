FROM debian:latest

RUN apt-get update && apt-get install -y openssh-server curl
RUN mkdir /var/run/sshd
RUN echo 'root:root123' | chpasswd
RUN useradd regular
RUN echo 'regular:regular' | chpasswd
RUN echo 'PermitRootLogin yes\nSubsystem sftp internal-sftp' > /etc/ssh/sshd_config

EXPOSE 22

CMD ["/usr/sbin/sshd", "-D"]